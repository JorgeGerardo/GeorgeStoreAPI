using Dapper;
using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Carts;
using GeorgeStore.Features.PaymentMethods;
using GeorgeStore.Features.Products;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GeorgeStore.Features.Orders;
public partial class OrderService(IDbConnectionFactory connection, GeorgeStoreContext context, KeyedAsyncLock locker, ICartRepository cartRep) : IOrderService
{
    public async Task<Result<int>> Purchase(Guid UserId, int CartId, int AddressId, int PaymentMethodId)
    {
        await using var _ = await locker.AcquireAsync(UserId.ToString(), TimeSpan.FromSeconds(30));

        var result = await cartRep.GetAsync(UserId, CancellationToken.None);
        if(!result.IsSuccess)
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


    public async Task<PagedResult<OrderDto>> Get(Guid UserId, QueryParams Prms)
    {
        var conn = connection.CreateConnection();
        const string query = """
                WITH OrdersPaged AS (
                    SELECT 
                        O.Id, O.UserId, O.DateUtc, O.Total, O.Status,
                        O.Street, O.Neighborhood, O.City, O.State, O.PostalCode,
                        O.ExternalNumber, O.InternalNumber, O.[References],
                        O.CardHolderName, O.Last4, O.Brand

                    FROM Orders AS O
                    WHERE O.UserId = @UserId
                      AND (
                            @Term IS NULL
                            OR EXISTS (
                                SELECT 1
                                FROM OrderDetails OD
                                INNER JOIN Products P ON P.Id = OD.ProductId
                                WHERE OD.OrderId = O.Id
                                  AND P.Name LIKE @Term
                            )
                          )
                    ORDER BY O.DateUtc DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY
                )
                SELECT
                    O.Id, O.UserId, O.DateUtc, O.Total, O.Status,
                    O.Street, O.Neighborhood, O.City, O.State, O.PostalCode,
                    O.ExternalNumber, O.InternalNumber, O.[References],
                    O.CardHolderName, O.Last4, O.Brand,
                    OD.Id, OD.OrderId, OD.ProductId, OD.UnitPrice, OD.SubTotal, OD.Quantity,
                    P.[Image], P.[Name]
                FROM OrdersPaged AS O 
                    INNER JOIN OrderDetails AS OD ON O.Id = OD.OrderId
                    INNER JOIN Products AS P ON P.Id = OD.ProductId
                ORDER BY O.DateUtc DESC
            """;
        var dic = new Dictionary<int, OrderDto>();

        await conn.QueryAsync<OrderDto, OrderDetailDto, OrderDto>(
            query,
            (order, detail) =>
            {
                if (!dic.TryGetValue(order.Id, out OrderDto? current))
                {
                    current = order;
                    dic.Add(order.Id, order);
                }

                current.Details.Add(detail);

                return current;
            },
            new { UserId, Prms.Offset, Prms.PageSize, Term = $"%{Prms.Term}%" },
            splitOn: "Id"
        );

        int total = Prms.Term is not null
            ? await GetTotal(Prms, UserId, conn)
            : await context.Orders.CountAsync(p => p.UserId == UserId);

        return new PagedResult<OrderDto>(dic.Values, total);
    }

    public async Task<Result<OrderDto>> GetByIdAsync(Guid UserId, int OrderId)
    {
        var conn = connection.CreateConnection();
        Dictionary<int, OrderDto> OrderDic = [];
        const string query = """
                SELECT
                    O.Id, O.UserId, O.DateUtc, O.Total, O.Status,
                    O.Street, O.Neighborhood, O.City, O.State, O.PostalCode,
                    O.ExternalNumber, O.InternalNumber, O.[References],
                    O.CardHolderName, O.Last4, O.Brand,
                    OD.Id, OD.OrderId, OD.ProductId, OD.UnitPrice, OD.SubTotal, OD.Quantity,
                    P.[Image], P.[Name]
                FROM Orders AS O
                    INNER JOIN OrderDetails AS OD ON O.Id = OD.OrderId
                    INNER JOIN Products AS P ON P.Id = OD.ProductId
                WHERE O.UserId = @UserId
                    AND O.Id = @OrderId
                ORDER BY O.DateUtc DESC
            """;

        await conn.QueryAsync<OrderDto, OrderDetailDto, OrderDto>(
            query,
            (order, detail) =>
            {
                if (!OrderDic.TryGetValue(order.Id, out OrderDto? current))
                {
                    OrderDic.Add(order.Id, order);
                    current = order;
                }

                current.Details.Add(detail);

                return current;
            },
            new { UserId, OrderId },
            splitOn: "Id"
        );

        OrderDto? order = OrderDic.Values.FirstOrDefault();
        return order is null
            ? Result.Failure<OrderDto>(OrderError.Notfound)
            : Result.Success(order);
    }

    private static async Task<int> GetTotal(QueryParams prms, Guid UserId, IDbConnection conn)
    {
        string query = """
             SELECT COUNT(*) from Orders AS O
                 INNER JOIN OrderDetails AS OD ON O.Id = OD.OrderId
                 INNER JOIN Products AS P ON OD.ProductId = P.Id
             WHERE UserId = @UserId 
                 AND P.Name LIKE @Term
            """;
        return await conn.ExecuteScalarAsync<int>(query, new { Term = $"%{prms.Term}%", UserId });
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
