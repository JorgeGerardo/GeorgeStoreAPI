using Dapper;
using GeorgeStore.Common.Shared;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeorgeStore.Features.Carts;

public class CartRepository(GeorgeStoreContext context, IDbConnectionFactory dbConnection, KeyedAsyncLock locker) : ICartRepository
{
    public async Task<Result<Cart>> GetAsync(Guid UserId, CancellationToken ct = default)
    {
        Cart? cart = await GetActiveCart(UserId, ct);

        if (cart is Cart ac && ac.Items.Any(i => !i.Item.IsActive))
            await RemoveInvalidItems(ac);

        if (cart is not null)
            return Result.Success(cart);

        cart = CreateDraft(UserId);
        await context.SaveChangesAsync(ct);
        cart.Items = [.. cart.Items.Where(i => i.Item.IsActive)];
        return Result.Success(cart);
    }

    public async Task<Result> AddAsync(Guid UserId, int ProductId, int Quantity, CancellationToken ct = default)
    {
        await using var _ = await locker.AcquireAsync(UserId.ToString(), TimeSpan.FromSeconds(10), ct);
        Cart? cart = await GetActiveCart(UserId, ct, true);

        cart ??= CreateDraft(UserId);

        CartItem? cartItem = cart.Items.FirstOrDefault(p => p.ProductId == ProductId);

        bool existProduct = await context.Products.AnyAsync(p => p.Id == ProductId && p.IsActive, ct);
        if (!existProduct)
            return Result.Failure(CartError.ProductNotfound);

        if (cartItem is not null)
            cartItem.Quantity += Quantity;
        else
            cart.Items.Add(new CartItem
            {
                ProductId = ProductId,
                Quantity = Quantity
            });

        await context.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> RemoveAsync(Guid UserId, int ProductId, CancellationToken ct = default)
    {
        await using var _ = await locker.AcquireAsync(UserId.ToString(), TimeSpan.FromSeconds(10), ct);

        Cart? cart = await GetActiveCart(UserId, ct);

        if (cart is null)
            return Result.Failure(CartError.Notfound);

        CartItem? cartItem = cart.Items.FirstOrDefault(p => p.ProductId == ProductId);

        if (cartItem is null)
            return Result.Failure(CartError.ItemNotfound);

        cart.Items.Remove(cartItem);
        await context.SaveChangesAsync(ct);
        return Result.Success();
    }

    private Cart CreateDraft(Guid UserId)
    {
        var newCart = Cart.Create(UserId);
        context.Carts.Add(newCart);
        return newCart;
    }

    public async Task<int> CountAsync(Guid UserId)
    {
        var connection = dbConnection.CreateConnection();
        const string query = """
                SELECT SUM(CI.Quantity) from Carts AS C
                    INNER JOIN CartItems as CI
                    ON CI.CartId = C.Id
                    INNER JOIN Products AS P ON P.Id = CI.ProductId
                    WHERE UserId = @UserId AND C.[Status] = @Status AND P.IsActive = 1
            """;

        return await connection.ExecuteScalarAsync<int>(query, new { UserId, Status = CartStatus.Active });
    }

    public async Task<Result> DecreaseAsync(Guid UserId, int ProductId)
    {
        await using var _ = await locker.AcquireAsync(UserId.ToString(), TimeSpan.FromSeconds(10));
        Cart? cart = await GetActiveCart(UserId, CancellationToken.None, true);

        if (cart is null)
            return Result.Failure(CartError.Notfound);

        CartItem? cartItem = cart.Items.FirstOrDefault(p => p.ProductId == ProductId);
        if (cartItem is null)
            return Result.Failure(CartError.ItemNotfound);

        if (cartItem.Quantity == 1)
            return Result.Failure(CartError.DecreaseLimit);

        if (cartItem is not null)
            cartItem.Quantity--;


        await context.SaveChangesAsync();
        return Result.Success();
    }

    private async Task<Cart?> GetActiveCart(Guid UserId, CancellationToken ct, bool onlyActiveProducts = false)
    {
        IQueryable<Cart> query = context.Carts;

        if (onlyActiveProducts)
            query = query
                .Include(c => c.Items.Where(i => i.Item.IsActive)).ThenInclude(i => i.Item);
        else
            query = query.Include(c => c.Items).ThenInclude(i => i.Item);

        return await query.FirstOrDefaultAsync(
            c => c.UserId == UserId && c.Status == CartStatus.Active, ct
        );
    }

    private async Task<Result> RemoveInvalidItems(Cart cart)
    {
        cart.Items.RemoveAll(i => !i.Item.IsActive);
        await context.SaveChangesAsync();
        return Result.Success();

    }
}
