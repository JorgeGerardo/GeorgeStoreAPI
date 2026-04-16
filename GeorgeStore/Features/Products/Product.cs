using GeorgeStore.Common;
using GeorgeStore.Features.Categories;

namespace GeorgeStore.Features.Products;

public class Product : Entity
{
    public required string Name { get; set; }
    public float Price { get; set; }
    public required string Description { get; set; }
    public required string Image { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public ProductDto ToDTO()
    {
        return new(Id, Name, Price, Description, Image, CategoryId, Category.Name);
    }
}
