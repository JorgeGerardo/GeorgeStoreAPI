using GeorgeStore.Common.Core;
using GeorgeStore.Features.Products;

namespace GeorgeStore.Features.Categories;

public class Category : Entity
{
    public required string Name { get; set; }
    public string? Image { get; set; }
    public List<Product> Products { get; set; } = [];
}
