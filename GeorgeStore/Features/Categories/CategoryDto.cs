namespace GeorgeStore.Features.Categories;


public record CategoryDto(int Id, string Name, string? Image)
{
    public static CategoryDto FromEntity(Category entity) 
        => new(entity.Id, entity.Name, entity.Image);
}