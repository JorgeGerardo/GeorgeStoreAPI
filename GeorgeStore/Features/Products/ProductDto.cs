namespace GeorgeStore.Features.Products;

public record ProductDto(int Id, string Name, float Price, string Description, string Image, int CategoryId, string CategoryName);

public record ProductCreateDTO(string Name, float Price, string Description, string Image, int CategoryId);
