using Bogus;
using GeorgeStore.Features.Products;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using System.Data;

namespace GeorgeStore.Tests.Factories;

internal static class ProductFactory
{
    public static ProductRepository CreateRepository(GeorgeStoreContext context)
    {
        IDbConnection sqlConn = ContextHelper.CreateSqlConn(context);
        var connFactory = ContextHelper.CreateConnectionFactory(sqlConn);
        return new(connFactory.Object, context);
    }

    public static Product Create(GeorgeStoreContext context, decimal price, bool isActive = true)
    {
        Faker faker = new("es_MX");
        Product product = new()
        {
            Name = faker.Commerce.ProductName(),
            Price = price,
            IsActive = isActive,
            Category = CategoryFactory.CreateRandom(context),
            Description = faker.Commerce.ProductDescription(),
            Image = faker.Image.PicsumUrl()
        };
        context.Add(product);
        context.SaveChanges();
        return product;

    }

}
