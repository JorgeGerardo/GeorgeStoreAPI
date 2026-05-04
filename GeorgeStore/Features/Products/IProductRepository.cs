using GeorgeStore.Common.Shared;

namespace GeorgeStore.Features.Products;

public interface IProductRepository
{
    Task<PagedResult<ProductDto>> GetProductsAsync(QueryParams prms);
    Task<Result<Product>> GetByIdAsync(int id);
    Task<Result> CreateAsync(ProductCreateDTO request);
    Task<Result> RemoveAsync(int id);
    Task<bool> ExistAsync(int id);
}

