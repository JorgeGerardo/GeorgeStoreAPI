using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Categories;
using GeorgeStore.Features.Orders;
using GeorgeStore.Features.Products;
using GeorgeStore.Infrastructure.Data;
using Moq;

namespace GeorgeStore.Tests.Factories;

internal static class OrderFactory
{
    public static OrderService CreateOrderService(GeorgeStoreContext context)
    {
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();
        return new OrderService(connectionFactoryMock.Object, context, new KeyedAsyncLock());

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

    public static Product CreateProduct(string name, decimal price, bool isActive = true)
    {
        return new Product
        {
            Name = name,
            Price = price,
            IsActive = isActive,
            Category = new Category { Name = "Cat" }, //Change to current method or CategoryFactory
            Description = "",
            Image = ""
        };
    }
    //TODO: Change to Bugo
    public static Order CreateOrder(Guid userId, params OrderDetail[] details)
    {
        return new Order
        {
            UserId = userId,
            Details = [.. details],
            Total = details.Sum(d => d.SubTotal),
            Brand = "Visa",
            Last4 = "4030",
            PostalCode = "830302",
            CardHolderName = "Test",
            City = "NW",
            State = "SA",
            DateUtc = DateTime.UtcNow,
            Street = "Street",
            Neighborhood = "Col"
        };
    }

    //Change to AddressFactory.CreateRandomAddress
    public static Address CreateAddress(Guid userId, string alias, bool isDefault = false)
    {
        return new Address()
        {
            Alias = alias,
            UserId = userId,
            Street = "Default street",
            Neighborhood = "default neighborhood",
            City = "Default city",
            State = "Default state",
            PostalCode = "Default postalCode",
            ExternalNumber = "68",
            InternalNumber = "88",
            References = "Default references",
            IsDefault = isDefault
        };
    }

}
