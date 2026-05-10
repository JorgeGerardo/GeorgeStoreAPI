using Bogus;
using GeorgeStore.Features.Carts;
using GeorgeStore.Features.Categories;
using GeorgeStore.Infrastructure.Data;
using Moq;

namespace GeorgeStore.Tests.Factories;

internal static class CategoryFactory
{
    public static CartRepository CreateCartRepository(GeorgeStoreContext context)
    {
        var connFactory = new Mock<IDbConnectionFactory>();
        return new(context, connFactory.Object, new KeyedAsyncLock());
    }

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