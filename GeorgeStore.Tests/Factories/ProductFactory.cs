using GeorgeStore.Features.Categories;
using GeorgeStore.Features.Products;
using GeorgeStore.Infrastructure.Data;
using Moq;

namespace GeorgeStore.Tests.Factories;

internal static class ProductFactory
{
    public static ProductRepository CreateRepository(GeorgeStoreContext context)
    {
        var connFactory = new Mock<IDbConnectionFactory>();
        return new(connFactory.Object, context);
    }

    public static Category CreateRandomCategory(GeorgeStoreContext context)
    {
        Category newCategory = new() { Name = "Electronic", Image = "" };
        context.Add(newCategory);
        context.SaveChanges();
        return newCategory;
    }


}
