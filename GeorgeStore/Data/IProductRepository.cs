using GeorgeStore.Core;
using GeorgeStore.Models;
using GeorgeStore.Services;

namespace GeorgeStore.Data;

public interface IProductRepository
{
    Task<IEnumerable<ProductDto>> GetProducts(QueryParams prms);
    Task<Result<ProductDto>> GetById(int id);
    Task<Result> Create(ProductCreateDTO request);
    Task<Result> Delete(int id);
    Task<bool> Exist(int id);
}

