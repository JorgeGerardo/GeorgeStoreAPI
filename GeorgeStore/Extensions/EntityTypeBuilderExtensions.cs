using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace GeorgeStore.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static void SeedFromJson<TEntity>(this EntityTypeBuilder<TEntity> builder, string fileName) where TEntity : class
    {
        JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        var userFile = File.ReadAllText(Path.Combine("Infrastructure", "DataSeed", fileName));
        var entities = JsonSerializer.Deserialize<List<TEntity>>(userFile, options: options);
        if (entities is null || entities.Count is 0)
            throw new Exception($"{fileName} isn't have a data");
        builder.HasData(entities);

    }
}
