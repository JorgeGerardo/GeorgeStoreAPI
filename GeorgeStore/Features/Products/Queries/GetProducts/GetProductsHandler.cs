using Dapper;
using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Common.Shared;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;

namespace GeorgeStore.Features.Products.Queries.GetProducts;

public class GetProductsHandler(IDbConnectionFactory dbConnection, GeorgeStoreContext context) : IQueryHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery query)
    {
        using var conn = dbConnection.CreateConnection();
        QueryParams prms = query.prms;
        StringBuilder sql = new("""
            SELECT
                p.id,
                p.name,
                p.price,
                p.description,
                p.image,
                p.category_id,
                c.name as category_name
            FROM products as p
                INNER JOIN categories as c on p.category_id = c.id
            WHERE is_active = true 
        """);
        if (prms.Term is not null)
            sql.Append(""" AND p.name ILIKE @term """);

        sql.Append(" LIMIT @pageSize OFFSET @offset ");
        var x = sql.ToString();
        var products = await conn.QueryAsync<ProductDto>(sql.ToString(), new { term = $"%{prms.Term}%", prms.Offset, prms.PageSize });
        int total = query.prms.Term is not null
            ? await GetTotal(query.prms, conn)
            : await context.Products.CountAsync(p => p.IsActive);

        return new PagedResult<ProductDto>(products, total);
    }

    private static async Task<int> GetTotal(QueryParams prms, IDbConnection conn)
    {
        const string query = """SELECT COUNT(*) FROM products WHERE is_active = true AND name ILIKE @Term""";
        return await conn.ExecuteScalarAsync<int>(query, new { Term = $"%{prms.Term}%" });
    }

}
