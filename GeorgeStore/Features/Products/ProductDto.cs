namespace GeorgeStore.Features.Products;

public record ProductDto(int Id, string Name, decimal Price, string Description, string Image, int CategoryId, string CategoryName)
{
    public static ProductDto FromEntity(Product entity)
    {
        return new(entity.Id, entity.Name, entity.Price, entity.Description, entity.Image, entity.CategoryId, entity.Category.Name);
    }
}

public record ProductCreateDTO(string Name, decimal Price, string Description, string Image, int CategoryId);
