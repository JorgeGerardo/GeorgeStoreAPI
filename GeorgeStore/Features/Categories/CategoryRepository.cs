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
            SELECT id, name, image FROM categories
                ORDER BY id
                LIMIT @PageSize OFFSET @Offset
        """;

        return await conn.QueryAsync<Category>(query, new { prms.Offset, prms.PageSize });
    }
}