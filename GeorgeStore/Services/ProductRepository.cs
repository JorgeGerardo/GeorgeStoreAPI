using Dapper;
using GeorgeStore.Core;
using GeorgeStore.Data;
using GeorgeStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GeorgeStore.Services;

public class ProductRepository(IDbConnectionFactory dbConnection, AppDbContext context) : IProductRepository
{
    public async Task<Result> Create(ProductCreateDTO request)
    {
        Product newProdcut = new()
        {
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Image = request.Image,
            Price = request.Price,
        };

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



    public async Task<Result<ProductDto>> GetById(int id)
    {
        Product? product = await context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return Result.Failure<ProductDto>(ProductError.Notfound);

        return Result.Success(
                new ProductDto(
                    product.Id, 
                    product.Name, 
                    product.Price, 
                    product.Description, 
                    product.Image, 
                    product.CategoryId, 
                    product.Category!.Name
                )
        );
    }

    public async Task<IEnumerable<ProductDto>> GetProducts(QueryParams prms)
    {
        using var conn = dbConnection.CreateConnection();
        StringBuilder query = new("""
                SELECT p.Id, p.Name, p.Price, p.[Description], p.[Image], p.CategoryId, c.Name as [categoryName] FROM Products as p
                INNER JOIN Categories as c on p.CategoryId = c.Id 
            """);
        if (prms.Term is not null)
            query.Append($"WHERE p.Name like ('%{prms.Term}%')");

        query.Append("""
                    ORDER by p.Id
                    OFFSET @offset Rows
                    FETCH NEXT @pageSize ROWS ONLY
        """);
        return await conn.QueryAsync<ProductDto>(query.ToString(), new { prms.Offset, prms.PageSize });
    }

}

public record QueryParams(int PageSize = 10, int Offset = 0, string? Term = null);

