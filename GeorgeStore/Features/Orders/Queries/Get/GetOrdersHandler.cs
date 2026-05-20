using Dapper;
using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Common.Shared;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GeorgeStore.Features.Orders.Queries.Get;

public sealed class GetOrdersHandler(IDbConnectionFactory _db, GeorgeStoreContext context) : IQueryHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery query)
    {
        var Prms = query.Prms;
        var conn = _db.CreateConnection();
        const string sql = """
                WITH "OrdersPaged" AS (
                    SELECT 
                        o.id,
                        o.user_id,
                        o.date_utc,
                        o.total,
                        o.status,
                        o.street,
                        o.neighborhood,
                        o.city,
                        o.state,
                        o.postal_code,
                        o.external_number,
                        o.internal_number,
                        o."references",
                        o.card_holder_name,
                        o.last4,
                        o.brand

                    FROM orders AS O
                    WHERE O.user_id = @UserId
                      AND (
                            @Term IS NULL
                            OR EXISTS (
                                SELECT 1
                                FROM order_details OD
                                INNER JOIN products P ON P.id = OD.product_id
                                WHERE OD.order_id = O.id
                                  AND P.name ILIKE @Term
                            )
                          )
                    ORDER BY O.date_utc DESC
                    LIMIT @PageSize OFFSET @Offset
                )
                SELECT
                    o.id, o.user_id, o.date_utc, o.total, o.status,
                    o.street, o.neighborhood, o.city, o.state, o.postal_code,
                    o.external_number, o.internal_number, o."references",
                    o.card_holder_name, o.last4, o.brand,
                    od.id, od.order_id, od.product_id, od.unit_price, od.sub_total, od.quantity,
                    p.image, p.name
                FROM "OrdersPaged" AS O 
                    INNER JOIN order_details AS OD ON O.id = OD.order_id
                    INNER JOIN products AS P ON P.id = OD.product_id
                ORDER BY O.date_utc DESC

            """;
        var dic = new Dictionary<int, OrderDto>();

        await conn.QueryAsync<OrderDto, OrderDetailDto, OrderDto>(
            sql,
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
            new { query.UserId, Prms.Offset, Prms.PageSize, Term = $"%{Prms.Term}%" },
            splitOn: "Id"
        );

        int total = Prms.Term is not null
            ? await GetOrdersCountAsync(Prms, query.UserId, conn)
            : await context.Orders.CountAsync(p => p.UserId == query.UserId);

        return new PagedResult<OrderDto>(dic.Values, total);
    }

    private static async Task<int> GetOrdersCountAsync(QueryParams prms, Guid UserId, IDbConnection conn)
    {
        string query = """
                SELECT COUNT(*) 
                FROM orders AS o
                    INNER JOIN order_details AS od ON o.id = od.order_id
                    INNER JOIN products AS p ON od.product_id = p.id
                WHERE user_id = @UserId
                    AND p.name ILIKE @Term
            """;
        return await conn.ExecuteScalarAsync<int>(query, new { Term = $"%{prms.Term}%", UserId });
    }

}
