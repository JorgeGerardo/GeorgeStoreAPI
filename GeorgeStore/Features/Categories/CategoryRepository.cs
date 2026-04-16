using Dapper;
using GeorgeStore.Common;
using GeorgeStore.Infrastructure.Data;
using System.Text;

namespace GeorgeStore.Features.Categories;

public class CategoryRepository(IDbConnectionFactory dbConnection) : ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetAsync(QueryParams prms)
    {
        using var conn = dbConnection.CreateConnection();
        StringBuilder query = new("""
            SELECT Id, Name, [Image] FROM Categories
                ORDER BY Id
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
        """);

        return await conn.QueryAsync<Category>(query.ToString(), new { prms.Offset, prms.PageSize });
    }
}