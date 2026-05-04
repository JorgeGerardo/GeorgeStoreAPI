using GeorgeStore.Common.Core;
using GeorgeStore.Features.Categories;

namespace GeorgeStore.Features.Products;

public class Product : Entity
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public required string Description { get; set; }
    public required string Image { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public static Product Create( string name, string description, int categoryId, string image, decimal price)
    {
        return new Product
        {
            Name = name,
            Description = description,
            CategoryId = categoryId,
            Image = image,
            Price = price,
        };
    }
}
