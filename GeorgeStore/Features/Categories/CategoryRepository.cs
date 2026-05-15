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
            SELECT "Id", "Name", "Image" FROM "Categories"
                ORDER BY "Id"
                LIMIT @PageSize OFFSET @Offset
        """;

        return await conn.QueryAsync<Category>(query, new { prms.Offset, prms.PageSize });
    }
}