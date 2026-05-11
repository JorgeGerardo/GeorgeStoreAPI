using Bogus;
using GeorgeStore.Features.Carts;
using GeorgeStore.Features.Orders;
using GeorgeStore.Features.Products;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using System.Data;

namespace GeorgeStore.Tests.Factories;

internal static class OrderFactory
{
    public static OrderService CreateService(GeorgeStoreContext context)
    {
        IDbConnection sqlConn = ContextHelper.CreateSqlConn(context);
        var connFactory = ContextHelper.CreateConnectionFactory(sqlConn);
        KeyedAsyncLock locker = new();
        ICartRepository cartRep = new CartRepository(context, connFactory.Object, locker);
        return new OrderService(connFactory.Object, context, locker, cartRep);
    }

    public static OrderDetail CreateOrderDetail(Product product, int qty, decimal unitPrice, decimal subTotal)
    {
        return new OrderDetail
        {
            Product = product,
            Quantity = qty,
            UnitPrice = unitPrice,
            SubTotal = subTotal
        };
    }

    public static Order Create(Guid userId, params OrderDetail[] details)
    {
        Faker faker = new("es_MX");
        return new Order
        {
            UserId = userId,
            Details = [.. details],
            Total = details.Sum(d => d.SubTotal),
            Brand = faker.PickRandom("Visa", "Mastercard", "American Express"),
            Last4 = faker.Random.ReplaceNumbers("####"),
            PostalCode = faker.Address.ZipCode(),
            CardHolderName = faker.Name.FullName(),
            City = faker.Address.City(),
            State = faker.Address.State(),
            DateUtc = DateTime.UtcNow,
            Street = faker.Address.StreetAddress(),
            Neighborhood = faker.Address.County(),
        };
    }


}