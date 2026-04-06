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
        return new ProductDto(Id, Name, Price, Description, Image, CategoryId, categoryName);
    }
}
public record ProductDto(int id, string Name, float Price, string Description, string Image, int CategoryId, string categoryName);

public record ProductCreateDTO(string name, float price, string description, string image, int categoryId);

