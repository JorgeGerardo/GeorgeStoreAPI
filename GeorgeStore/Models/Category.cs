namespace GeorgeStore.Models;

public class Category : Entity
{
    public required string Name { get; set; }
    public float Price { get; set; }
    public required string Description { get; set; }
    public string? Image { get; set; }
    public IEnumerable<Product> Products { get; set; } = new List<Product>();
}