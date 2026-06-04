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
        ProductQueryParams prms = query.Prms;
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

        if (query.Prms.CategoryId is not null)
            sql.Append(""" AND p.category_id = @categoryId """);

        sql.Append(" LIMIT @pageSize OFFSET @offset ");

        var products = await conn.QueryAsync<ProductDto>(sql.ToString(), new { term = $"%{prms.Term}%", prms.Offset, prms.PageSize, categoryId = prms.CategoryId });
        int total = query.Prms.Term is not null || query.Prms.CategoryId is not null
            ? await GetTotal(query.Prms, conn)
            : await context.Products.CountAsync(p => p.IsActive);

        return new PagedResult<ProductDto>(products, total);
    }

    private static async Task<int> GetTotal(ProductQueryParams prms, IDbConnection conn)
    {
        StringBuilder sql = new("""SELECT COUNT(*) FROM products as p WHERE is_active = true AND p.name ILIKE @Term  """);
        if (prms.CategoryId is not null)
            sql.Append(" AND p.category_id = @categoryId ");

        return await conn.ExecuteScalarAsync<int>(sql.ToString(), new { Term = $"%{prms.Term}%", categoryId = prms.CategoryId });
    }

}
