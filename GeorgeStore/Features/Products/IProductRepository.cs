using GeorgeStore.Common;

namespace GeorgeStore.Features.Products;

public interface IProductRepository
{
    Task<PagedResult<ProductDto>> GetProducts(QueryParams prms);
    Task<Result<Product>> GetById(int id);
    Task<Result> Create(ProductCreateDTO request);
    Task<Result> Delete(int id);
    Task<bool> Exist(int id);
}

