using Dapper;
using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Infrastructure.Data;

namespace GeorgeStore.Features.Carts.Query.GetCartItemsCount;

public sealed class GetCartItemsCountHandler(IDbConnectionFactory dbConnection) : IQueryHandler<GetCartItemsCountQuery, int>
{
    public async Task<int> Handle(GetCartItemsCountQuery query)
    {
        var connection = dbConnection.CreateConnection();
        const string sql = """
                SELECT COALESCE(SUM(CI.quantity), 0) from carts AS C
                  INNER JOIN cart_items as CI
            	    ON CI.cart_id = C.id
                  INNER JOIN products AS P ON P.id = CI.product_id
                WHERE user_id = @UserId AND C.status = @Status AND P.is_active = true
            """;

        return await connection.ExecuteScalarAsync<int>(sql, new {
            query.UserId, 
            Status = CartStatus.Active }
        );
    }
}
