using GeorgeStore.Features.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace GeorgeStore.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        var productFile = File.ReadAllText(Path.Combine("Infrastructure", "DataSeed", "Product.json"));
        var products = JsonSerializer.Deserialize<List<Product>>(productFile, options: options);
        if (products is null || products.Count is 0)
            throw new Exception("Product Data isn't exist");
        builder.HasData(products);
    }
}
