using GeorgeStore.Common.Shared;

namespace GeorgeStore.Features.Categories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAsync(QueryParams prms);
}
