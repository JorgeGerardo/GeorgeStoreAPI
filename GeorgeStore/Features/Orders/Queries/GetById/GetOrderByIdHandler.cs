using Dapper;
using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Common.Shared;
using GeorgeStore.Infrastructure.Data;

namespace GeorgeStore.Features.Orders.Queries.GetById;

public sealed class GetOrderByIdHandler(IDbConnectionFactory _db) : IQueryHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery query)
    {
        var conn = _db.CreateConnection();
        Dictionary<int, OrderDto> OrderDic = [];
        const string sql = """
                SELECT
                    o.id, o.user_id, o.date_utc, o.total, o.status,
                    o.street, o.neighborhood, o.city, o.state, o.postal_code,
                    o.external_number, o.internal_number, o."references",
                    o.card_holder_name, o.last4, o.brand,
                    od.id, od.order_id, od.product_id, od.unit_price, od.sub_total, od.quantity,
                    p.image, p.name
                FROM orders AS o
                    INNER JOIN order_details AS od ON o.id = od.order_id
                    INNER JOIN products AS p ON p.id = od.product_id
                WHERE o.user_id = @UserId
                    AND o.id = @OrderId
                ORDER BY o.date_utc DESC
            """;

        await conn.QueryAsync<OrderDto, OrderDetailDto, OrderDto>(
            sql,
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
            new { query.UserId, query.OrderId },
            splitOn: "id"
        );

        OrderDto? order = OrderDic.Values.FirstOrDefault();
        return order is null
            ? Result.Failure<OrderDto>(OrderError.Notfound)
            : Result.Success(order);
    }

}