using GeorgeStore.Features.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace GeorgeStore.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        var categoryFile = File.ReadAllText(Path.Combine("Infrastructure", "DataSeed", "Category.json"));
        var categories = JsonSerializer.Deserialize<List<Category>>(categoryFile, options: options);
        if (categories is null || categories.Count is 0)
            throw new Exception("Category Data isn't exist");
        builder.HasData(categories);
    }
}
