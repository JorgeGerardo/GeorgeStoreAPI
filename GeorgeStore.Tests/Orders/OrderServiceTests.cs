using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Carts;
using GeorgeStore.Features.Orders;
using GeorgeStore.Features.PaymentMethods;
using GeorgeStore.Features.Products;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using GeorgeStore.Tests.Factories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GeorgeStore.Tests.Orders;

public class OrderServiceTests
{

    [Theory]  //P1 $,    Qty,P2 $, Qty Total
    [InlineData(10_000.0, 2, 500.0, 5, 22_500.0)]
    [InlineData(6_000.0,  5, 500.0, 5, 32_500.0)]
    public async Task PreviewReorder_Success(decimal Product1Price, int QtyP1, decimal Product2Price, int QtyP2, decimal Total)
    {
        decimal subP1 = Product1Price * QtyP1;
        decimal subP2 = Product2Price * QtyP2;

        // Arrange
        using var context = ContextHelper.Create();
        OrderService orderService = OrderFactory.CreateService(context);
        User user = ContextHelper.CreateUser(context);

        Product product1 = ProductFactory.Create(context, Product1Price);
        Product product2 = ProductFactory.Create(context, Product2Price);
        Product product3 = ProductFactory.Create(context, 1000m, isActive: false);

        Order order = OrderFactory.Create(
            user.Id,
            OrderFactory.CreateOrderDetail(product1, QtyP1, 8000m, 16_000m),
            OrderFactory.CreateOrderDetail(product2, QtyP2, 4500m, 4500m),
            OrderFactory.CreateOrderDetail(product3, 3, 900m, 2700m)
        );

        context.Add(order);
        context.SaveChanges();


        //Act
        var result = await orderService.PreviewReorder(user.Id, order.Id);

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
        using var context = ContextHelper.Create();
        OrderService orderService = OrderFactory.CreateService(context);

        var user = ContextHelper.CreateUser(context);
        Product product1 = ProductFactory.Create(context, Product1Price);
        Product product2 = ProductFactory.Create(context, Product2Price);
        Product product3 = ProductFactory.Create(context, 1000m, isActive: false);

        var order = OrderFactory.Create(
            user.Id,
            OrderFactory.CreateOrderDetail(product1, QtyP1, 8000m, 16_000m),
            OrderFactory.CreateOrderDetail(product2, QtyP2, 4500m, 4500m),
            OrderFactory.CreateOrderDetail(product3, 3, 900m, 2700m)
        );

        context.Add(order);
        context.SaveChanges();



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
        using var context = ContextHelper.Create();
        OrderService orderService = OrderFactory.CreateService(context);

        User user = ContextHelper.CreateUser(context);
        Product product1 = ProductFactory.Create(context, 1321m, isActive: false);
        Product product2 = ProductFactory.Create(context, 31232m, isActive: false);
        Product product3 = ProductFactory.Create(context, 1000m, isActive: false);

        var order = OrderFactory.Create(
            user.Id,
            OrderFactory.CreateOrderDetail(product1, 1, 8000m, 16_000m),
            OrderFactory.CreateOrderDetail(product2, 1, 4500m, 4500m),
            OrderFactory.CreateOrderDetail(product3, 3, 900m, 2700m)
        );

        context.Add(order);
        context.SaveChanges();


        //Act
        var result = await orderService.PreviewReorder(user.Id, order.Id);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OrderError.NoValidItems, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }


    [Theory]  //P1 $,    Qty,P2 $, Qty Total
    [InlineData(10_000.0, 2, 500.0, 5, 22_500.0)]
    [InlineData(6_000.0,  5, 500.0, 5, 32_500.0)]
    public async Task Reorder_WithOneInvalidProduct(decimal Product1Price, int QtyP1, decimal Product2Price, int QtyP2, decimal Total)
    {
        //Arrange
        using var context = ContextHelper.Create();
        OrderService orderSvc = OrderFactory.CreateService(context);

        decimal subP1 = Product1Price * QtyP1;
        decimal subP2 = Product2Price * QtyP2;
        User user = ContextHelper.CreateUser(context);
        Address address = AddressFactory.CreateRandom(context, user, isDefault: false);

        var result = PaymentMethod.Create(user.Id, "1234123412341234", "", 1, 2030, "");
        Assert.True(result.IsSuccess);
        PaymentMethod paymentM = result.Value;

        Product product1  = ProductFactory.Create(context, Product1Price);
        Product product2 = ProductFactory.Create(context, Product2Price);
        Product product3 = ProductFactory.Create(context, 1000m, isActive: false);

        Order order = OrderFactory.Create(
            user.Id,
            new OrderDetail { Product = product1, Quantity = QtyP1, UnitPrice = 8000m, SubTotal = 16_000m },
            new OrderDetail { Product = product2, Quantity = QtyP2, UnitPrice = 4500m, SubTotal = 4500m },
            new OrderDetail { Product = product3, Quantity = 3,     UnitPrice = 900m,  SubTotal = 2700m }
        );

        context.AddRange([order, paymentM]);
        context.SaveChanges();

        //Act
        context.ChangeTracker.Clear();
        ReorderRequest request = new(order.Id, address.Id, paymentM.Id);
        var orderResult = await orderSvc.ReorderAsync(user.Id, request);
        Order? orderCreated = context.Orders.Include(p => p.Details)
                                         .ThenInclude(d => d.Product)
                                         .FirstOrDefault(o => o.Id == orderResult.Value);

        //Assert
        Assert.NotNull(orderCreated);
        Assert.Equal(Total, orderCreated.Total);
        Assert.Equal(2, orderCreated.Details.Count);

        decimal subtotal1 = orderCreated.Details.First(i => i.ProductId == product1.Id).SubTotal;
        decimal subtotal2 = orderCreated.Details.First(i => i.ProductId == product2.Id).SubTotal;
        Assert.Equal(subP1, subtotal1);
        Assert.Equal(subP2, subtotal2);

    }

    [Theory]  //P1 $,    Qty,P2 $, Qty Total
    [InlineData(10_000.0, 2, 500.0, 5, 22_500.0)]
    [InlineData(6_000.0,  5, 500.0, 5, 32_500.0)]
    public async Task Purchase(decimal Product1Price, int QtyP1, decimal Product2Price, int QtyP2, decimal Total)
    {
        //Arrange
        using var context = ContextHelper.Create();
        OrderService orderSvc = OrderFactory.CreateService(context);

        decimal subP1 = Product1Price * QtyP1;
        decimal subP2 = Product2Price * QtyP2;
        User user = ContextHelper.CreateUser(context);
        Address address = AddressFactory.CreateRandom(context, user, isDefault: false);

        var pmResult = PaymentMethod.Create(user.Id, "1234123412341234", "", 1, 2030, "");
        Assert.True(pmResult.IsSuccess);
        PaymentMethod paymentM = pmResult.Value;


        Product product1 = ProductFactory.Create(context, Product1Price);
        Product product2 = ProductFactory.Create(context, Product2Price);
        context.SaveChanges();


        var activeCart = Cart.Create(user.Id);
        activeCart.Items = [
            new CartItem{
                ProductId = product1.Id,
                Quantity = QtyP1,
                Item = product1,
            },
            new CartItem{
                ProductId = product2.Id,
                Quantity = QtyP2,
                Item = product2
            },
        ];
        context.AddRange([activeCart, paymentM]);
        context.SaveChanges();

        //Act
        var result = await orderSvc.Purchase(user.Id, activeCart.Id, address.Id, paymentM.Id);
        int orderId = result.Value;

        //Assert
        Assert.True(result.IsSuccess);
        Order? order = context.Orders.Include(o => o.Details).FirstOrDefault(o => o.Id == orderId);
        Assert.NotNull(order);
        Assert.Equal(Total, order.Total);

        var orderDetailP1 = order.Details.FirstOrDefault(o => o.ProductId == product1.Id);
        Assert.NotNull(orderDetailP1);
        Assert.Equal(subP1, orderDetailP1.SubTotal);
        
        var orderDetailP2 = order.Details.FirstOrDefault(o => o.ProductId == product2.Id);
        Assert.NotNull(orderDetailP2);
        Assert.Equal(subP2, orderDetailP2.SubTotal);
        Assert.Equal(CartStatus.Converted, activeCart.Status);

    }


    [Theory]  //P1 $,    Qty,P2 $, Qty Total
    [InlineData(10_000.0, 2, 500.0, 5, 20_000.0)]
    [InlineData(6_000.0,  5, 500.0, 5, 30_000.0)]
    public async Task Purchase_WithProductsInactive(decimal Product1Price, int QtyP1, decimal Product2Price, int QtyP2, decimal Total)
    {
        //Arrange
        using var context = ContextHelper.Create();
        OrderService orderSvc = OrderFactory.CreateService(context);

        decimal subP1 = Product1Price * QtyP1;
        decimal subP2 = Product2Price * QtyP2;
        User user = ContextHelper.CreateUser(context);

        var pmResult = PaymentMethod.Create(user.Id, "1234123412341234", "Visa", 1, 2030, "Sofia L.");
        Assert.True(pmResult.IsSuccess);

        Address address = AddressFactory.CreateRandom(context, user, isDefault: false);
        PaymentMethod paymentM = pmResult.Value;
        Product product1 = ProductFactory.Create(context, Product1Price);
        Product product2 = ProductFactory.Create(context, Product2Price, isActive: false);

        Cart activeCart = Cart.Create(user.Id);
        activeCart.Items = [
            new CartItem{
                ProductId = product1.Id,
                Quantity = QtyP1,
                Item = product1,
            },
            new CartItem{
                ProductId = product2.Id,
                Quantity = QtyP2,
                Item = product2,
            },
        ];
        context.AddRange([activeCart, paymentM]);
        context.SaveChanges();

        //Act
        var result = await orderSvc.Purchase(user.Id, activeCart.Id, address.Id, paymentM.Id);
        int orderId = result.Value;

        //Assert
        Assert.True(result.IsSuccess);
        Order? order = context.Orders.Include(o => o.Details).FirstOrDefault(o => o.Id == orderId);
        Assert.NotNull(order);
        Assert.Equal(Total, order.Total);

        OrderDetail? orderDetailP1 = order.Details.FirstOrDefault(o => o.ProductId == product1.Id);
        Assert.NotNull(orderDetailP1);
        Assert.Equal(subP1, orderDetailP1.SubTotal);

        OrderDetail? orderDetailP2 = order.Details.FirstOrDefault(o => o.ProductId == product2.Id);
        Assert.Null(orderDetailP2);
        Assert.Equal(CartStatus.Converted, activeCart.Status);

    }


    [Fact]
    public async Task Purchase_Failure_AddressNotFound()
    {
        //Arrange
        using var context = ContextHelper.Create();
        OrderService orderSvc = OrderFactory.CreateService(context);
        User user = ContextHelper.CreateUser(context);

        var pmResult = PaymentMethod.Create(user.Id, "1234123412341234", "", 1, 2030, "");
        Assert.True(pmResult.IsSuccess);
        
        PaymentMethod paymentM = pmResult.Value;
        Product product1 = ProductFactory.Create(context, 1000m);
        Product product2 = ProductFactory.Create(context, 2000m);


        Cart newCart = Cart.Create(user.Id);
        newCart.Items = [
            new CartItem{
                ProductId = product1.Id,
                Quantity = 1,
                Item = product1,
            },
        ];
        context.AddRange(newCart, paymentM);
        context.SaveChanges();

        //Act
        var result = await orderSvc.Purchase(user.Id, newCart.Id, 2, paymentM.Id);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OrderError.AddressNotFound, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }


    [Fact]
    public async Task Purchase_Failure_PaymentMethodNotFound()
    {
        //Arrange
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        Address address = AddressFactory.CreateRandom(context, user, isDefault: false);
        OrderService orderSvc = OrderFactory.CreateService(context);
        Product product1 = ProductFactory.Create(context, 1000m);
        Product product2 = ProductFactory.Create(context, 2000m);

        Cart newCart = Cart.Create(user.Id);
        newCart.Items = [
            new CartItem{
                ProductId = product1.Id,
                Quantity = 1,
                Item = product1,
            },
        ];
        context.Add(newCart);
        context.SaveChanges();

        //Act
        var result = await orderSvc.Purchase(user.Id, newCart.Id, address.Id, 1);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(OrderError.PaymentMethodNotFound, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }



    [Fact]
    public async Task Reorder_Failure_AddressNotFound()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        OrderService orderSvc = OrderFactory.CreateService(context);
        Product product1 = ProductFactory.Create(context, 30123m);

        Order order = OrderFactory.Create(
            user.Id,
            new OrderDetail { Product = product1, Quantity = 1, UnitPrice = 8000m, SubTotal = 16_000m }
        );

        context.Add(order); 
        context.SaveChanges();


        ReorderRequest request = new(order.Id, 1, 1);

        //act
        var result = await orderSvc.ReorderAsync(user.Id, request);
        Assert.False(result.IsSuccess);
        Assert.Equal(OrderError.AddressNotFound, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public async Task Reorder_Failure_PaymentMethodNotFound()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        OrderService orderSvc = OrderFactory.CreateService(context);
        Product newProduct = ProductFactory.Create(context, 30123m);

        Order order = OrderFactory.Create(
            user.Id,
            new OrderDetail { Product = newProduct, Quantity = 1, UnitPrice = 8000m, SubTotal = 16_000m }
        );

        Address address = AddressFactory.CreateRandom(context, user, isDefault: false);
        context.Add(order);
        context.SaveChanges();
        ReorderRequest request = new(order.Id, address.Id, 1);


        //act
        var result = await orderSvc.ReorderAsync(user.Id, request);
        Assert.False(result.IsSuccess);
        Assert.Equal(PaymentMethodError.NotFound, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public async Task Reorder_Failure_OrderNotFound()
    {
        var connectionFactoryMock = new Mock<IDbConnectionFactory>();
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        OrderService orderSvc = new(connectionFactoryMock.Object, context, new KeyedAsyncLock());
        ReorderRequest request = new(1, 1, 1);

        //act
        var result = await orderSvc.ReorderAsync(user.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(OrderError.Notfound, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

}
