using GeorgeStore.Models;
using GeorgeStore.Services;

namespace GeorgeStore.Data;

public interface IProductRepository
{
    Task<IEnumerable<ProductDto>> GetProducts(QueryParams prms);
    Task<ProductDto?> GetById(int id);
    Task<bool> Create(ProductCreateDTO request);
    Task<bool> Delete(int id);
    Task<bool> Exist(int id);
}

