using GeorgeStore.Common.Shared;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GeorgeStore.Features.Products;

public class ProductRepository(GeorgeStoreContext context) : IProductRepository
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

}

