using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Carts;
using GeorgeStore.Features.PaymentMethods;
using GeorgeStore.Features.Products;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GeorgeStore.Features.Orders;
public partial class OrderService(GeorgeStoreContext context, KeyedAsyncLock locker, ICartRepository cartRep) : IOrderService
{
    public async Task<Result<int>> Purchase(Guid UserId, int CartId, int AddressId, int PaymentMethodId)
    {
        await using var _ = await locker.AcquireAsync(UserId.ToString(), TimeSpan.FromSeconds(30));

        var result = await cartRep.GetAsync(UserId, CancellationToken.None);
        if(result.IsFailure)
            return Result.Failure<int>(OrderError.CartNotNotfound);

        Cart cart = result.Value;

        Address? address = await context.Addresses
            .FirstOrDefaultAsync(addr => addr.UserId == UserId && addr.Id == AddressId);

        PaymentMethod? paymentMethod = await context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.UserId == UserId && pm.Id == PaymentMethodId);

        if (address is null)
            return Result.Failure<int>(OrderError.AddressNotFound);

        if (paymentMethod is null)
            return Result.Failure<int>(OrderError.PaymentMethodNotFound);

        cart.Status = CartStatus.Converted;
        Order newOrder = CreateOrder(cart, UserId, address, paymentMethod);
        context.Orders.Add(newOrder);

        await context.SaveChangesAsync();
        return Result.Success(newOrder.Id);
    }

    public async Task<Result<int>> ReorderAsync(Guid UserId, ReorderRequest request)
    {
        Order? order = await context.Orders
            .Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.UserId == UserId && o.Id == request.OrderId);
        if (order is null)
            return Result.Failure<int>(OrderError.Notfound);

        Address? address = await context.Addresses
            .FirstOrDefaultAsync(addr => addr.UserId == UserId && addr.Id == request.AddressId);
        if (address is null)
            return Result.Failure<int>(OrderError.AddressNotFound);

        PaymentMethod? paymentMethod = await context.PaymentMethods
            .FirstOrDefaultAsync(pm => pm.UserId == UserId && pm.Id == request.PaymentMethodId);
        if (paymentMethod is null)
            return Result.Failure<int>(PaymentMethodError.NotFound);

        var productIdList = order.Details.Select(d => d.ProductId).ToList();

        var products = await context.Products
            .Where(p => productIdList.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        var validDetails = order.Details
            .Where(d => d.Product != null && d.Product.IsActive)
            .ToList();

        if (validDetails.Count == 0)
            return Result.Failure<int>(OrderError.NoValidItems);


        Order orderClone = CreateReorder(order, address, paymentMethod);

        context.Orders.Add(orderClone);
        await context.SaveChangesAsync();
        return Result.Success(orderClone.Id);
    }

    public async Task<Result<ReorderPreview>> PreviewReorder(Guid UserId, int OrderId)
    {
        Order? order = await context.Orders
            .Include(o => o.Details)
            .ThenInclude(d => d.Product)
            .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(o => o.UserId == UserId && o.Id == OrderId);

        if (order is null)
            return Result.Failure<ReorderPreview>(OrderError.Notfound);

        var validDetails = order.Details
            .Where(d => d.Product.IsActive)
            .Select(d =>
            {//Update price if was changed (Not saved, just preview)
                d.UnitPrice = d.Product.Price;
                d.SubTotal = d.Product.Price * d.Quantity;
                return d;
            })
            .ToList();

        if (validDetails.Count == 0)
            return Result.Failure<ReorderPreview>(OrderError.NoValidItems);

        var invalidItems = order.Details
            .Where(d => d.Product == null || !d.Product.IsActive)
            .Select(i => ProductDto.FromEntity(i.Product));

        var total = validDetails
            .Sum(d => d.Product.Price * d.Quantity);

        var items = validDetails.Select(OrderDetailDto.FromEntity);

        return Result.Success(new ReorderPreview(OrderId, items, total, invalidItems));
    }

}

public partial class OrderService : IOrderService
{
    private static Order CreateReorder(Order oldOrder, Address address, PaymentMethod paymentMethod)
    {
        List<OrderDetail> details = [];

        foreach (var detail in oldOrder.Details)
        {
            var product = detail.Product;

            if (product is null || !product.IsActive)
                continue;

            details.Add(new OrderDetail
            {
                ProductId = product.Id,
                Product = product,
                Quantity = detail.Quantity,
                UnitPrice = product.Price,
                SubTotal = detail.Quantity * product.Price
            });
        }

        return new Order
        {
            UserId = oldOrder.UserId,
            Details = details,
            Total = details.Sum(d => d.Quantity * d.UnitPrice),

            // Address snapshot
            Street = address.Street,
            Neighborhood = address.Neighborhood,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            ExternalNumber = address.ExternalNumber,
            InternalNumber = address.InternalNumber,
            References = address.References,

            // Payment snapshot
            CardHolderName = paymentMethod.CardHolderName,
            Last4 = paymentMethod.LastDigits,
            Brand = paymentMethod.Brand,
        };
    }

    private static Order CreateOrder(Cart cart, Guid UserId, Address Address, PaymentMethod PaymentMethod)
    {
        List<OrderDetail> details = [];
        foreach (var item in cart.Items.Where(i => i.Item.IsActive))
            details.Add(
                OrderDetail.Create(
                    item.ProductId,
                    Convert.ToDecimal(item.Item.Price),
                    Convert.ToDecimal(item.Item.Price * item.Quantity),
                    item.Quantity
                )
            );

        return Order.Create(UserId, details.Sum(v => v.SubTotal), Address, PaymentMethod, details);
    }

}
