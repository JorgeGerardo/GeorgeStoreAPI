using Bogus;
using GeorgeStore.Features.Categories;
using GeorgeStore.Infrastructure.Data;

namespace GeorgeStore.Tests.Factories;

internal static class CategoryFactory
{
    public static Category CreateRandom(GeorgeStoreContext context)
    {
        Faker faker = new("es_MX");
        Category category = new()
        {
            Name = faker.Commerce.Categories(1).First(),
            Image = faker.Image.PicsumUrl()
        };
        context.Add(category);
        context.SaveChanges();
        return category;
    }

}