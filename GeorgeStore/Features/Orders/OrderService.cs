using Dapper;
using GeorgeStore.Common;
using GeorgeStore.Features.Carts;
using GeorgeStore.Infrastructure.Data;
using System;

namespace GeorgeStore.Features.Orders;

public class OrderService(IDbConnectionFactory connection) : IOrderService
{
    public Task<Result> Buy(Cart cart)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<OrderDto>> Get(Guid UserId, QueryParams Prms)
    {
        var conn = connection.CreateConnection();
        const string query = """
                WITH OrdersPaged AS (
                    SELECT 
                        O.Id, O.UserId, O.DateUtc, O.Total, O.Status 
                    FROM Orders AS O
                    WHERE UserId = @UserId
                    ORDER BY DateUtc DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize  ROWS ONLY
                )
                SELECT
                    O.Id, O.UserId, O.DateUtc, O.Total, O.Status,
                    OD.Id, OD.OrderId, OD.ProductId, OD.UnitPrice, OD.SubTotal, OD.Quantity,
                    P.[Image]
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
            new { UserId, Prms.Offset, Prms.PageSize },
            splitOn: "Id"
        );

        return dic.Values;
    }

    public async Task<Result<OrderDto>> GetById(Guid UserId, int OrderId)
    {
        var conn = connection.CreateConnection();
        Dictionary<int, OrderDto> OrderDic = [];
        const string query = """
                SELECT
                    O.Id, O.UserId, O.DateUtc, O.Total, O.Status,
                    OD.Id, OD.OrderId, OD.ProductId, OD.UnitPrice, OD.SubTotal, OD.Quantity,
                    P.[Image]
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
}

public class OrderError
{
    public static readonly Error Notfound =
        new("Order not found", "Can't find any order with this id", "Order.Notfound");
}


