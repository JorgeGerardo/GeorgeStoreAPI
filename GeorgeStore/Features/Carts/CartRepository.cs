
using GeorgeStore.Common;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeorgeStore.Features.Carts;

public class CartRepository(GeorgeStoreContext context, KeyedAsyncLock locker) : ICartRepository
{
    public async Task<Result<Cart>> GetAsync(Guid UserId, CancellationToken ct = default)
    {
        Cart? cart = await context.Carts
                                .Include(c => c.Items)
                                .ThenInclude(i => i.Item)
                                .FirstOrDefaultAsync(c => c.UserId == UserId && c.Status == CartStatus.Draft, ct);

        if (cart is null)
            return Result.Failure<Cart>(CartError.Notfound);

        return Result.Success(cart);
    }

    public async Task<Result> AddAsync(Guid UserId, int ProductId, uint Quantity, CancellationToken ct = default)
    {
        await using var _ = await locker.AcquireAsync(UserId.ToString(), TimeSpan.FromSeconds(10), ct);
        Cart? cart = await context.Carts
                                .Include(p => p.Items)
                                .FirstOrDefaultAsync(c => c.UserId == UserId && c.Status == CartStatus.Draft, ct);

        cart ??= CreateDraft(UserId);

        CartItem? cartItem = cart.Items.FirstOrDefault(p => p.ProductId == ProductId);

        if (cartItem is not null)
            cartItem.Quantity += Quantity;
        else
            cart.Items.Add(new CartItem
            {
                ProductId = ProductId,
                Quantity = Quantity
            });

        return await context.SaveChangesAsync(ct) > 0
            ? Result.Success()
            : Result.Failure(CartError.UnexpectedError);
    }

    public async Task<Result> RemoveAsync(Guid UserId, int ProductId, CancellationToken ct = default)
    {
        await using var _ = await locker.AcquireAsync(UserId.ToString(), TimeSpan.FromSeconds(10), ct);

        Cart? cart = await context.Carts.Include(p => p.Items).FirstOrDefaultAsync(c => c.UserId == UserId, ct);
        if (cart is null)
            return Result.Failure(CartError.Notfound);

        CartItem? cartItem = cart.Items.FirstOrDefault(p => p.ProductId == ProductId);

        if (cartItem is null)
            return Result.Failure(CartError.ItemNotfound);


        cart.Items.Remove(cartItem);
        return await context.SaveChangesAsync(ct) > 0 
            ? Result.Success() 
            : Result.Failure(CartError.UnexpectedError);
    }

    private Cart CreateDraft(Guid UserId)
    {
        var newCart = new Cart
        {
            UserId = UserId,
            Items = [],
            Status = CartStatus.Draft,
        };
        context.Carts.Add(newCart);

        return newCart;
    }
}
