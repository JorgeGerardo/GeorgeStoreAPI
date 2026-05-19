using Dapper;
using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Infrastructure.Data;

namespace GeorgeStore.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesHandler(IDbConnectionFactory _db): IQueryHandler<GetCategoriesQuery, IEnumerable<Category>>
{
    public async Task<IEnumerable<Category>> Handle(GetCategoriesQuery query)
    {
        using var conn = _db.CreateConnection();

        const string sql = """
            SELECT id, name, image
            FROM categories
            ORDER BY id
            LIMIT @PageSize OFFSET @Offset
        """;

        return await conn.QueryAsync<Category>(sql, new
        {
            query.Params.Offset,
            query.Params.PageSize
        });
    }
}