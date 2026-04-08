using GeorgeStore.Core;

namespace GeorgeStore.Models;

public class Product : Entity
{
    public required string Name { get; set; }
    public float Price { get; set; }
    public required string Description { get; set; }
    public required string Image { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ProductDto ToDTO(string categoryName)
    {
        return new(Id, Name, Price, Description, Image, CategoryId, categoryName);
    }
}
public record ProductDto(int Id, string Name, float Price, string Description, string Image, int CategoryId, string CategoryName);

public record ProductCreateDTO(string Name, float Price, string Description, string Image, int CategoryId);



public static class ProductError
{
    public static readonly Error Notfound =
        new("Product not found", "Can't find product selected", "Product.Notfound");
    public static readonly Error Conflict =
        new("Has occurred an error", "Try again in some times", "Product.Conflict");

}