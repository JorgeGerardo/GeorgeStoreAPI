using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Carts;
using GeorgeStore.Features.Categories;
using GeorgeStore.Features.Orders;
using GeorgeStore.Features.PaymentMethods;
using GeorgeStore.Features.Products;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GeorgeStore.Tests.OrderTests;

public class OrderServiceTest
{
    [Theory]  //P1 $,    Qty,P2 $, Qty Total
    [InlineData(10_000.0, 2, 500.0, 5, 22_500.0)]
    [InlineData(6_000.0,  5, 500.0, 5, 32_500.0)]
    public async Task PreviewReorder_Success(decimal Product1Price, int QtyP1, decimal Product2Price, int QtyP2, decimal Total)
    {
        var subP1 = Product1Price * QtyP1;
        var subP2 = Product2Price * QtyP2;
        // Arrange
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();
        var locker = new KeyedAsyncLock();

        using var context = ContextHelper.Create();

        var userId = Guid.NewGuid();
        var product1 = CreateProduct("Laptop", Product1Price, isActive: true);
        var product2 = CreateProduct("Smarthphone", Product2Price, isActive: true);
        var product3 = CreateProduct("Fridge", 1000m, isActive: false);

        var order = CreateOrder(
            userId,
            new OrderDetail { Product = product1, Quantity = QtyP1, UnitPrice = 8000m, SubTotal = 16_000m },
            new OrderDetail { Product = product2, Quantity = QtyP2, UnitPrice = 4500m, SubTotal = 4500m },
            new OrderDetail { Product = product3, Quantity = 3, UnitPrice = 900m, SubTotal = 2700m }
        );

        context.Orders.Add(order);
        await context.SaveChangesAsync();


        var orderService = new OrderService(connectionFactoryMock.Object, context, locker);

        //Act
        var result = await orderService.PreviewReorder(userId, order.Id);

        //Assert
        Assert.True(result.IsSuccess);
        var preview = result.Value;
        Assert.Equal(Total, preview.Total);
        Assert.NotEmpty(preview.Items);
        Assert.Single(preview.InvalidItems);

        var subtotal1 = preview.Items.First(i => i.Id == product1.Id).SubTotal;
        var subtotal2 = preview.Items.First(i => i.Id == product2.Id).SubTotal;
        Assert.Equal(subP1, subtotal1);
        Assert.Equal(subP2, subtotal2);
    }

    [Theory]  //P1 $,    Qty,P2 $, Qty Total
    [InlineData(10_000.0, 2, 500.0, 6, 23_000.0)]
    [InlineData(6_000.0,  5, 500.0, 5, 32_500.0)]
    public async Task PreviewReorder_Success_When_InvalidProducts_Are_In(decimal Product1Price, int QtyP1, decimal Product2Price, int QtyP2, decimal Total)
    {
        // Arrange
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();
        var locker = new KeyedAsyncLock();
        using var context = ContextHelper.Create();

        var user = ContextHelper.CreateUser(context);
        var product1 = CreateProduct("Laptop", Product1Price, isActive: true);
        var product2 = CreateProduct("Smarthphone", Product2Price, isActive: true);
        var product3 = CreateProduct("Fridge", 1000m, isActive: false);

        var order = CreateOrder(
            user.Id,
            new OrderDetail { Product = product1, Quantity = QtyP1, UnitPrice = 8000m, SubTotal = 16_000m },
            new OrderDetail { Product = product2, Quantity = QtyP2, UnitPrice = 4500m, SubTotal = 4500m },
            new OrderDetail { Product = product3, Quantity = 3, UnitPrice = 900m, SubTotal = 2700m }
        );

        context.Orders.Add(order);
        await context.SaveChangesAsync();


        var orderService = new OrderService(connectionFactoryMock.Object, context, locker);

        //Act
        var result = await orderService.PreviewReorder(user.Id, order.Id);

        //Assert
        Assert.True(result.IsSuccess);
        var preview = result.Value;
        Assert.Equal(2, preview.Items.Count());
        Assert.Single(preview.InvalidItems);
        Assert.Equal(Total, preview.Total);

        Assert.Contains(preview.InvalidItems, i => i.Id == product3.Id);
    }


    [Fact]
    public async Task PreviewReorder_Failure_When_Order_HasNotHaveActiveProducts()
    {
        // Arrange
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();
        var locker = new KeyedAsyncLock();
        using var context = ContextHelper.Create();

        var user = ContextHelper.CreateUser(context);
        var product1 = CreateProduct("Laptop",      1321m,  isActive: false);
        var product2 = CreateProduct("Smarthphone", 31232m, isActive: false);
        var product3 = CreateProduct("Fridge",      1000m,  isActive: false);
        context.AddRange([product1, product2, product3]);
        context.SaveChanges();

        var order = CreateOrder(
            user.Id,
            new OrderDetail { Product = product1, Quantity = 1, UnitPrice = 8000m, SubTotal = 16_000m },
            new OrderDetail { Product = product2, Quantity = 1, UnitPrice = 4500m, SubTotal = 4500m },
            new OrderDetail { Product = product3, Quantity = 3, UnitPrice = 900m, SubTotal = 2700m }
        );

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var orderService = new OrderService(connectionFactoryMock.Object, context, locker);

        //Act
        var result = await orderService.PreviewReorder(user.Id, order.Id);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OrderError.NoValidItems, result.Error);
    }


    [Theory]  //P1 $,    Qty,P2 $, Qty Total
    [InlineData(10_000.0, 2, 500.0, 5, 22_500.0)]
    [InlineData(6_000.0,  5, 500.0, 5, 32_500.0)]
    public async Task Reorder_WithOneInvalidProduct(decimal Product1Price, int QtyP1, decimal Product2Price, int QtyP2, decimal Total)
    {
        //Arrange
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();
        using var context = ContextHelper.Create();

        var subP1 = Product1Price * QtyP1;
        var subP2 = Product2Price * QtyP2;
        var user = ContextHelper.CreateUser(context);
        Address address = CreateAddress(user.Id, "Work", false);

        var result = PaymentMethod.Create(user.Id, "1234123412341234", "", 1, 2030, "");
        Assert.True(result.IsSuccess);
        PaymentMethod paymentM = result.Value;


        var product1  = CreateProduct("Laptop", Product1Price, isActive: true);
        var product2 = CreateProduct("Smarthphone", Product2Price, isActive: true);
        var product3 = CreateProduct("Fridge", 1000m, isActive: false);

        var order = CreateOrder(
            user.Id,
            new OrderDetail { Product = product1, Quantity = QtyP1, UnitPrice = 8000m, SubTotal = 16_000m },
            new OrderDetail { Product = product2, Quantity = QtyP2, UnitPrice = 4500m, SubTotal = 4500m },
            new OrderDetail { Product = product3, Quantity = 3,     UnitPrice = 900m,  SubTotal = 2700m }
        );


        context.Orders.Add(order);
        context.Addresses.Add(address);
        context.PaymentMethods.Add(paymentM);
        context.SaveChanges();

        //Act
        context.ChangeTracker.Clear();
        OrderService orderSvc = new(connectionFactoryMock.Object, context, new KeyedAsyncLock());
        var request = new ReorderRequest(order.Id, address.Id, paymentM.Id);
        var orderResult = await orderSvc.ReorderAsync(user.Id, request);
        var orderCreated = context.Orders.Include(p => p.Details)
                                         .ThenInclude(d => d.Product)
                                         .FirstOrDefault(o => o.Id == orderResult.Value);

        //Assert
        Assert.NotNull(orderCreated);
        Assert.Equal(Total, orderCreated.Total);
        Assert.Equal(2, orderCreated.Details.Count);

        var subtotal1 = orderCreated.Details.First(i => i.ProductId == product1.Id).SubTotal;
        var subtotal2 = orderCreated.Details.First(i => i.ProductId == product2.Id).SubTotal;
        Assert.Equal(subP1, subtotal1);
        Assert.Equal(subP2, subtotal2);

    }

    [Theory]  //P1 $,    Qty,P2 $, Qty Total
    [InlineData(10_000.0, 2, 500.0, 5, 22_500.0)]
    [InlineData(6_000.0,  5, 500.0, 5, 32_500.0)]
    public async Task Purchase(decimal Product1Price, int QtyP1, decimal Product2Price, int QtyP2, decimal Total)
    {
        //Arrange
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();
        using var context = ContextHelper.Create();

        var subP1 = Product1Price * QtyP1;
        var subP2 = Product2Price * QtyP2;
        var user = ContextHelper.CreateUser(context);
        Address address = CreateAddress(user.Id, "Work", false);

        var pmResult = PaymentMethod.Create(user.Id, "1234123412341234", "", 1, 2030, "");
        Assert.True(pmResult.IsSuccess);
        PaymentMethod paymentM = pmResult.Value;

        context.Addresses.Add(address);
        context.PaymentMethods.Add(paymentM);
        context.SaveChanges();


        var product1 = CreateProduct("Laptop Asus", Product1Price);
        var product2 = CreateProduct("Smarthphone", Product2Price);
        context.Products.AddRange(product1, product2);
        context.SaveChanges();


        var activeCart = Cart.Create(user.Id);
        activeCart.Items = [
            new CartItem{
                ProductId = product1.Id,
                Quantity = QtyP1
            },
            new CartItem{
                ProductId = product2.Id,
                Quantity = QtyP2
            },
        ];
        context.Carts.Add(activeCart);
        context.SaveChanges();

        //Act
        OrderService orderSvc = new(connectionFactoryMock.Object, context, new KeyedAsyncLock());
        var result = await orderSvc.Purchase(user.Id, activeCart.Id, address.Id, paymentM.Id);
        int orderId = result.Value;

        //Assert
        Assert.True(result.IsSuccess);
        var order = context.Orders.Include(o => o.Details).FirstOrDefault(o => o.Id == orderId);
        Assert.NotNull(order);
        Assert.Equal(Total, order.Total);

        var detailProduct1 = order.Details.FirstOrDefault(o => o.ProductId == product1.Id);
        Assert.NotNull(detailProduct1);
        Assert.Equal(subP1, detailProduct1.SubTotal);
        
        var detailProduct2 = order.Details.FirstOrDefault(o => o.ProductId == product2.Id);
        Assert.NotNull(detailProduct2);
        Assert.Equal(subP2, detailProduct2.SubTotal);
        Assert.Equal(CartStatus.Converted, activeCart.Status);

    }


    [Fact]
    public async Task Purchase_Failure_AddressNotFound()
    {
        //Arrange
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();
        using var context = ContextHelper.Create();
        var user = ContextHelper.CreateUser(context);

        var pmResult = PaymentMethod.Create(user.Id, "1234123412341234", "", 1, 2030, "");
        Assert.True(pmResult.IsSuccess);
        PaymentMethod paymentM = pmResult.Value;

        context.PaymentMethods.Add(paymentM);
        context.SaveChanges();


        var product1 = CreateProduct("Laptop Asus", 1000m);
        var product2 = CreateProduct("Smarthphone", 2000m);
        context.Products.AddRange(product1, product2);
        context.SaveChanges();


        var newCart = Cart.Create(user.Id);
        newCart.Items = [
            new CartItem{
                ProductId = product1.Id,
                Quantity = 1
            },
        ];
        context.Carts.Add(newCart);
        context.SaveChanges();

        //Act
        OrderService orderSvc = new(connectionFactoryMock.Object, context, new KeyedAsyncLock());
        var result = await orderSvc.Purchase(user.Id, newCart.Id, 2, paymentM.Id);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OrderError.AddressNotFound, result.Error);
    }


    [Fact]
    public async Task Purchase_Failure_PaymentMethodNotFound()
    {
        //Arrange
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();
        using var context = ContextHelper.Create();
        var user = ContextHelper.CreateUser(context);
        Address address = CreateAddress(user.Id, "Work", false);
        context.Addresses.Add(address);
        context.SaveChanges();



        var product1 = CreateProduct("Laptop Asus", 1000m);
        var product2 = CreateProduct("Smarthphone", 2000m);
        context.Products.AddRange(product1, product2);
        context.SaveChanges();


        var newCart = Cart.Create(user.Id);
        newCart.Items = [
            new CartItem{
                ProductId = product1.Id,
                Quantity = 1
            },
        ];
        context.Carts.Add(newCart);
        context.SaveChanges();

        //Act
        OrderService orderSvc = new(connectionFactoryMock.Object, context, new KeyedAsyncLock());
        var result = await orderSvc.Purchase(user.Id, newCart.Id, address.Id, 1);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OrderError.PaymentMethodNotFound, result.Error);
    }



    [Fact]
    public async Task Reorder_Failure_AddressNotFound()
    {
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();
        using var context = ContextHelper.Create();
        var user = ContextHelper.CreateUser(context);
        OrderService orderSvc = new(connectionFactoryMock.Object, context, new KeyedAsyncLock());
        var product1 = CreateProduct("Laptop", 30123m, isActive: true);

        var order = CreateOrder(
            user.Id,
            new OrderDetail { Product = product1, Quantity = 1, UnitPrice = 8000m, SubTotal = 16_000m }
        );

        context.Orders.Add(order); 
        context.SaveChanges();


        ReorderRequest request = new(order.Id, 1, 1);

        //act
        var result = await orderSvc.ReorderAsync(user.Id, request);


        Assert.False(result.IsSuccess);
        Assert.Equal(OrderError.AddressNotFound, result.Error);

    }

    [Fact]
    public async Task Reorder_Failure_PaymentMethodNotFound()
    {
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();
        using var context = ContextHelper.Create();
        var user = ContextHelper.CreateUser(context);
        OrderService orderSvc = new(connectionFactoryMock.Object, context, new KeyedAsyncLock());
        var newProduct = CreateProduct("Laptop", 30123m, isActive: true);

        var order = CreateOrder(
            user.Id,
            new OrderDetail { Product = newProduct, Quantity = 1, UnitPrice = 8000m, SubTotal = 16_000m }
        );

        Address address = CreateAddress(user.Id, "Work", false);
        context.AddRange(address, order);
        context.SaveChanges();


        ReorderRequest request = new(order.Id, address.Id, 1);

        //act
        var result = await orderSvc.ReorderAsync(user.Id, request);


        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentMethodError.NotFound, result.Error);

    }

    [Fact]
    public async Task Reorder_Failure_OrderNotFound()
    {
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();
        using var context = ContextHelper.Create();
        Guid userId = Guid.NewGuid();
        OrderService orderSvc = new(connectionFactoryMock.Object, context, new KeyedAsyncLock());
        ReorderRequest request = new(1, 1, 1);

        //act
        var result = await orderSvc.ReorderAsync(userId, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(OrderError.Notfound, result.Error);
    }



    //Common Arranges:
    private static Product CreateProduct(string name, decimal price, bool isActive = true)
    {
        return new Product
        {
            Name = name,
            Price = price,
            IsActive = isActive,
            Category = new Category { Name = "Cat" },
            Description = "",
            Image = ""
        };
    }

    private static Order CreateOrder(Guid userId, params OrderDetail[] details)
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

    private static Address CreateAddress(Guid userId, string alias, bool isDefault = false)
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
