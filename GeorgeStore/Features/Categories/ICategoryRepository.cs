using GeorgeStore.Common;

namespace GeorgeStore.Features.Categories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAsync(QueryParams prms);
}
