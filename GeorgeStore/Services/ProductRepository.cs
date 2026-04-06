using Dapper;
using GeorgeStore.Data;
using GeorgeStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GeorgeStore.Services;

public class ProductRepository(IDbConnectionFactory dbConnection, AppDbContext context) : IProductRepository
{
    public async Task<bool> Create(ProductCreateDTO request)
    {
        var newProdcut = new Product
        {
            Name = request.name,
            Description = request.description,
            CategoryId = request.categoryId,
            Image = request.image,
            Price = request.price,
        };

        context.Products.Add(newProdcut);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Delete(int id)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return false;

        product.IsActive = false;
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> Exist(int id)
    {
        return await context.Products.AnyAsync(p => p.Id == id);
    }



    public async Task<ProductDto?> GetById(int id)
    {
        Product? product = await context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return null;

        return new ProductDto(product.Id, product.Name, product.Price, product.Description, product.Image, product.CategoryId, product.Category!.Name);
    }

    public async Task<IEnumerable<ProductDto>> GetProducts(QueryParams prms)
    {
        using var conn = dbConnection.CreateConnection();
        StringBuilder query = new StringBuilder("""
                SELECT p.Id, p.Name, p.Price, p.[Description], p.[Image], p.CategoryId, c.Name as [categoryName] FROM Products as p
                INNER JOIN Categories as c on p.CategoryId = c.Id 
            """);
        if (prms.term is not null)
            query.Append($"WHERE p.Name like ('%{prms.term}%')");

        query.Append("""
                    ORDER by p.Id
                    OFFSET @offset Rows
                    FETCH NEXT @pageSize ROWS ONLY
        """);
        return await conn.QueryAsync<ProductDto>(query.ToString(), new { prms.offset, prms.pageSize });
    }

}

public record QueryParams(int pageSize = 10, int offset = 0, string? term = null);

