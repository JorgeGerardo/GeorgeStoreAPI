using Dapper;
using GeorgeStore.Common;
using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Carts;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GeorgeStore.Features.Orders;

public partial class OrderService(IDbConnectionFactory connection, GeorgeStoreContext context, KeyedAsyncLock locker) : IOrderService
{
    public async Task<Result> Buy(Guid UserId, int CartId, int AddressId)
    {
        await using var _ = await locker.AcquireAsync(UserId.ToString(), TimeSpan.FromSeconds(30));
        var cart = await context.Carts
            .Include(c => c.Items)
            .ThenInclude(c => c.Item)
            .FirstOrDefaultAsync(c => c.UserId == UserId && c.Id == CartId && c.Status == CartStatus.Active);

        var address = await context.Addresses
            .FirstOrDefaultAsync(addr => addr.UserId == UserId && addr.Id == AddressId);

        if (address is null)
            return Result.Failure(OrderError.AddressNotFound);

        if (cart is null)
            return Result.Failure(OrderError.CartNotNotfound);

        cart.Status = CartStatus.Converted;
        Order newOrder = CreateOrder(cart, UserId, address);
        context.Orders.Add(newOrder);

        return await context.SaveChangesAsync() > 0
            ? Result.Success()
            : Result.Failure(OrderError.UnexpectedError);
    }

    public async Task<PagedResult<OrderDto>> Get(Guid UserId, QueryParams Prms)
    {
        var conn = connection.CreateConnection();
        const string query = """
                WITH OrdersPaged AS (
                    SELECT 
                        O.Id, O.UserId, O.DateUtc, O.Total, O.Status 
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

    public async Task<Result<OrderDto>> GetById(Guid UserId, int OrderId)
    {
        var conn = connection.CreateConnection();
        Dictionary<int, OrderDto> OrderDic = [];
        const string query = """
                SELECT
                    O.Id, O.UserId, O.DateUtc, O.Total, O.Status,
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
        throw new NotImplementedException();
    }

    private async Task<int> GetTotal(QueryParams prms, Guid UserId, IDbConnection conn)
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

    private Order CreateOrder(Cart cart, Guid UserId, Address Address)
    {
        List<OrderDetail> details = [];
        foreach (var item in cart.Items)
            details.Add(
                OrderDetail.Create(
                    item.ProductId,
                    Convert.ToDecimal(item.Item.Price),
                    Convert.ToDecimal(item.Item.Price * item.Quantity),
                    item.Quantity
                )
            );

        return Order.Create(UserId, details.Sum(v => v.SubTotal), Address, details);
    }


}
