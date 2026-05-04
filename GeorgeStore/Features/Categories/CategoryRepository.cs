using Dapper;
using GeorgeStore.Common.Shared;
using GeorgeStore.Infrastructure.Data;

namespace GeorgeStore.Features.Categories;

public class CategoryRepository(IDbConnectionFactory dbConnection) : ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetAsync(QueryParams prms)
    {
        using var conn = dbConnection.CreateConnection();
        const string query = """
            SELECT Id, Name, [Image] FROM Categories
                ORDER BY Id
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY
        """;

        return await conn.QueryAsync<Category>(query, new { prms.Offset, prms.PageSize });
    }
}