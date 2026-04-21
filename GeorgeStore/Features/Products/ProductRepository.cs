using Dapper;
using GeorgeStore.Common;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GeorgeStore.Features.Products;

public class ProductRepository(IDbConnectionFactory dbConnection, GeorgeStoreContext context) : IProductRepository
{
    public async Task<Result> Create(ProductCreateDTO request)
    {
        Product newProdcut = Product.Create(request.Name, request.Description, request.CategoryId, request.Image, request.Price);

        context.Products.Add(newProdcut);
        return await context.SaveChangesAsync() > 0
            ? Result.Success()
            : Result.Failure(ProductError.Conflict);
    }

    public async Task<Result> Delete(int id)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return Result.Failure(ProductError.Notfound);

        product.IsActive = false;
        return await context.SaveChangesAsync() > 0
            ? Result.Success()
            : Result.Failure(ProductError.Conflict);
    }

    public async Task<bool> Exist(int id)
    {
        return await context.Products.AnyAsync(p => p.Id == id);
    }



    public async Task<Result<Product>> GetById(int id)
    {
        Product? product = await context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .FirstOrDefaultAsync(p => p.Id == id);


        if (product is null)
            return Result.Failure<Product>(ProductError.Notfound);

        return Result.Success(product);
    }

    public async Task<IEnumerable<ProductDto>> GetProducts(QueryParams prms)
    {
        using var conn = dbConnection.CreateConnection();
        StringBuilder query = new("""
                SELECT p.Id, p.Name, p.Price, p.[Description], p.[Image], p.CategoryId, c.Name as [categoryName] FROM Products as p
                INNER JOIN Categories as c on p.CategoryId = c.Id 
            """);
        if (prms.Term is not null)
            query.Append($"WHERE p.Name like @term");

        query.Append("""
                    ORDER by p.Id
                    OFFSET @offset Rows
                    FETCH NEXT @pageSize ROWS ONLY
        """);
        return await conn.QueryAsync<ProductDto>(query.ToString(), new { term = $"%{prms.Term}%", prms.Offset, prms.PageSize });
    }

}

