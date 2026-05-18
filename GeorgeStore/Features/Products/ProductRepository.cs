using Dapper;
using GeorgeStore.Common.Shared;
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
        return await context.Products.AnyAsync(p => p.Id == id && p.IsActive);
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
            query.Append(""" AND p.name ILIKE @term """);

        query.Append(" LIMIT @pageSize OFFSET @offset ");
        var x = query.ToString();
        var products = await conn.QueryAsync<ProductDto>(query.ToString(), new { term = $"%{prms.Term}%", prms.Offset, prms.PageSize });
        int total = prms.Term is not null
            ? await GetTotal(prms, conn)
            : await context.Products.CountAsync(p => p.IsActive);

        return new PagedResult<ProductDto>(products, total);
    }

    private static async Task<int> GetTotal(QueryParams prms, IDbConnection conn)
    {
        const string query = """SELECT COUNT(*) FROM products WHERE is_active = true AND name ILIKE @Term""";
        return await conn.ExecuteScalarAsync<int>(query, new { Term = $"%{prms.Term}%" });
    }

}

