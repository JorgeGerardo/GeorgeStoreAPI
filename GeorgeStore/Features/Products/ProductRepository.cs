using Dapper;
using GeorgeStore.Common;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;

namespace GeorgeStore.Features.Products;

public class ProductRepository(IDbConnectionFactory dbConnection, GeorgeStoreContext context) : IProductRepository
{
    public async Task<Result> CreateAsync(ProductCreateDTO request)
    {
        Product newProdcut = Product.Create(request.Name, request.Description, request.CategoryId, request.Image, request.Price);

        context.Products.Add(newProdcut);
        await context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveAsync(int id)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return Result.Failure(ProductError.Notfound);

        product.IsActive = false;
        await context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<bool> ExistAsync(int id)
    {
        return await context.Products.AnyAsync(p => p.Id == id);
    }

    public async Task<Result<Product>> GetByIdAsync(int id)
    {
        Product? product = await context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .FirstOrDefaultAsync(p => p.Id == id);


        if (product is null)
            return Result.Failure<Product>(ProductError.Notfound);

        return Result.Success(product);
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(QueryParams prms)
    {
        using var conn = dbConnection.CreateConnection();
        StringBuilder query = new("""
             SELECT
                p.Id, p.Name, p.Price, p.[Description], p.[Image], p.CategoryId, c.Name as [categoryName]
             FROM Products as p
                INNER JOIN Categories as c on p.CategoryId = c.Id 
            """);

        query.Append($"WHERE IsActive = 1");
        if (prms.Term is not null)
            query.Append($"AND p.Name like @term");

        query.Append("""
                ORDER by p.Id
                OFFSET @offset Rows
                FETCH NEXT @pageSize ROWS ONLY
        """);
        var products = await conn.QueryAsync<ProductDto>(query.ToString(), new { term = $"%{prms.Term}%", prms.Offset, prms.PageSize });
        int total = prms.Term is not null
            ? await GetTotal(prms, conn)
            : await context.Products.CountAsync(p => p.IsActive);

        return new PagedResult<ProductDto>(products, total);
    }

    private async Task<int> GetTotal(QueryParams prms, IDbConnection conn)
    {
        const string query = "SELECT COUNT(*) FROM Products WHERE IsActive = 1 AND [Name] like @Term";
        return await conn.ExecuteScalarAsync<int>(query, new { Term = $"%{prms.Term}%" });
    }

}

