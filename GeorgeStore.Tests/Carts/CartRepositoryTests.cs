using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Carts;
using GeorgeStore.Features.Categories;
using GeorgeStore.Features.Products;
using GeorgeStore.Features.Users;
using GeorgeStore.Tests.Common;
using GeorgeStore.Tests.Factories;

namespace GeorgeStore.Tests.Carts;

public class CartRepositoryTests
{
    [Fact]
    public async Task Get_CartNotExist()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);


        CartRepository cartRep = CategoryFactory.CreateCartRepository(context);
        var result = await cartRep.GetAsync(user.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Cart cart = result.Value;
        Assert.Empty(cart.Items);
        Assert.Equal(user.Id, cart.UserId);
        Assert.Equal(CartStatus.Active, cart.Status);
    }



    [Theory]
    [InlineData(10_000.0,  2.0,      500.0,    6,  23_000.0)]
    [InlineData(    10.0, 10.0,        5.0,   10,  150.0)]
    [InlineData(25_000.0,  2.0,   10_000.0,    6,  110_000.0)]
    public async Task Get(decimal Product1Price, int QtyP1, decimal Product4Price, int QtyP4, decimal Total)
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        CartRepository cartRep = CategoryFactory.CreateCartRepository(context);

        Product product1 = ProductFactory.Create(context, Product1Price);
        Product product2 = ProductFactory.Create(context, 3000);
        Product product3 = ProductFactory.Create(context, 7000);
        Product product4 = ProductFactory.Create(context, Product4Price);

        Cart oldCart = new(){
            UserId = user.Id,
            Status = CartStatus.Converted,
            Items = [
                new CartItem {
                    ProductId = product1.Id,
                    Quantity = 383,
                }
            ]
        };

        Cart newCart = Cart.Create(user.Id);

        context.AddRange([newCart, oldCart]);
        context.SaveChanges();

        //Act
        Result result1 = await cartRep.AddAsync(user.Id, product1.Id, QtyP1, CancellationToken.None);
        Assert.True(result1.IsSuccess);
        Result result2 = await cartRep.AddAsync(user.Id, product4.Id, QtyP4, CancellationToken.None);
        Assert.True(result2.IsSuccess);

        context.ChangeTracker.Clear();
        var result = await cartRep.GetAsync(user.Id, CancellationToken.None);
        Cart? userCart = result.Value;
        Assert.True(result.IsSuccess);
        Assert.NotNull(userCart);
        Assert.Equal(2, userCart.Items.Count);
        int totalProducts = QtyP1 + QtyP4;
        Assert.Equal(totalProducts, userCart.Items.Sum(i => i.Quantity));
        Assert.Contains(userCart.Items, i => i.ProductId == product1.Id);
        Assert.Contains(userCart.Items, i => i.ProductId == product4.Id);
        Assert.Equal(Total, userCart.Total);


    }


    [Theory]
    [InlineData(10_000.0, 2, 500.0, 6, 20_000.0)]
    [InlineData(10.0, 10, 50.0, 10, 100.0)]
    [InlineData(25_000.0, 2, 10_000.0, 6, 50_000.0)]
    public async Task RemoveTest(decimal Product1Price, int QtyP1, decimal Product4Price, int QtyP4, decimal Total)
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        CartRepository cartRep = CategoryFactory.CreateCartRepository(context);

        Product product1 = ProductFactory.Create(context, Product1Price);
        Product product2 = ProductFactory.Create(context, 3000);
        Product product3 = ProductFactory.Create(context, 7000);
        Product product4 = ProductFactory.Create(context, Product4Price);

        Cart newCart = Cart.Create(user.Id);
        context.Add(newCart);
        context.SaveChanges();



        Result result1 = await cartRep.AddAsync(user.Id, product1.Id, QtyP1, CancellationToken.None);
        Assert.True(result1.IsSuccess);
        Result result2 = await cartRep.AddAsync(user.Id, product4.Id, QtyP4, CancellationToken.None);
        Assert.True(result2.IsSuccess);

        //Act
        context.ChangeTracker.Clear();
        Result removeResult = await cartRep.RemoveAsync(user.Id, product4.Id, CancellationToken.None);
        Assert.True(removeResult.IsSuccess);


        var result = await cartRep.GetAsync(user.Id, CancellationToken.None);
        Cart? userCart = result.Value;
        Assert.True(result.IsSuccess);
        Assert.NotNull(userCart);
        Assert.Single(userCart.Items);
        Assert.Equal(QtyP1, userCart.Items.Sum(i => i.Quantity));
        Assert.Contains(userCart.Items, i => i.ProductId == product1.Id);
        Assert.DoesNotContain(userCart.Items, i => i.ProductId == product4.Id);
        Assert.Equal(Total, userCart.Total);

    }


    [Fact]
    public async Task RemoveTest_Failure_ItemNotFound()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);

        Category newCategory = CategoryFactory.CreateRandom(context);
        Product product1 = ProductFactory.Create(context, 5500);
        Product product2 = ProductFactory.Create(context, 3000);        
        Cart newCart = Cart.Create(user.Id);

        context.Add(newCart);
        context.SaveChanges();

        CartRepository cartRep = CategoryFactory.CreateCartRepository(context);
        Result result1 = await cartRep.AddAsync(user.Id, product1.Id, 1, CancellationToken.None);
        Assert.True(result1.IsSuccess);

        //Act
        Result removeResult = await cartRep.RemoveAsync(user.Id, product2.Id, CancellationToken.None);
        //Assert
        Assert.False(removeResult.IsSuccess);
        Assert.Equal(CartError.ItemNotfound, removeResult.Error);
    }

    [Fact]
    public async Task RemoveTest_Failure_CartNotfound()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        CartRepository cartRep = CategoryFactory.CreateCartRepository(context);

        //Act
        Result removeResult = await cartRep.RemoveAsync(user.Id, int.MaxValue, CancellationToken.None);

        Assert.False(removeResult.IsSuccess);
        Assert.Equal(CartError.Notfound, removeResult.Error);
    }


    [Theory]
    [MemberData(nameof(ProductsInCartCasesForDecreaseTest))]
    public async Task DecreaseTest(decimal Product1Price, int QtyP1, int DecreaseQtyP1, decimal Product4Price, int QtyP4, int DecreaseQtyP4, decimal Total)
    {
        if (DecreaseQtyP1 >= QtyP1 || DecreaseQtyP4 >= QtyP4)
            throw new ArgumentException("Decrease Qty and Qty is inconsistent, check Theory Data");

        int finalP1Qty = QtyP1 - DecreaseQtyP1;
        int finalP4Qty = QtyP4 - DecreaseQtyP4;
        decimal totalCalculating = (finalP1Qty * Product1Price) + (finalP4Qty * Product4Price);

        if (totalCalculating != Total)
            throw new ArgumentException("Total is inconsistent, check Theory Data");


        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);

        Category newCategory = CategoryFactory.CreateRandom(context);
        Product product1 = ProductFactory.Create(context, Product1Price);
        Product product2 = ProductFactory.Create(context, 3000);
        Product product3 = ProductFactory.Create(context, 7000);
        Product product4 = ProductFactory.Create(context, Product4Price);

        Cart oldCart = new() {
            UserId = user.Id,
            Status = CartStatus.Converted,
            Items = [
                new CartItem {
                    ProductId = product1.Id,
                    Quantity = 383,
                    Item = product1,
                }
            ]
        };
        Cart activeCart = Cart.Create(user.Id);

        context.AddRange([activeCart, oldCart]);
        context.SaveChanges();
        
        //Act
        context.ChangeTracker.Clear();
        CartRepository cartRep = CategoryFactory.CreateCartRepository(context);
        Result result1 = await cartRep.AddAsync(user.Id, product1.Id, QtyP1, CancellationToken.None);
        Assert.True(result1.IsSuccess);
        Result result2 = await cartRep.AddAsync(user.Id, product4.Id, QtyP4, CancellationToken.None);
        Assert.True(result2.IsSuccess);

        //Act
        for (int i = 0; i < DecreaseQtyP1; i++)
        {
            Result res = await cartRep.DecreaseAsync(user.Id, product1.Id);
            Assert.True(res.IsSuccess);
        }

        for (int i = 0; i < DecreaseQtyP4; i++)
        {
            Result res = await cartRep.DecreaseAsync(user.Id, product4.Id);
            Assert.True(res.IsSuccess);
        }




        var getCartResult = await cartRep.GetAsync(user.Id, CancellationToken.None);
        Assert.True(getCartResult.IsSuccess);
        Cart? currentCart = getCartResult.Value;

        Assert.Equal(Total, currentCart.Total);
        CartItem? item1 = currentCart.Items.FirstOrDefault(i => i.ProductId == product1.Id);
        Assert.NotNull(item1);
        Assert.Equal(finalP1Qty, item1.Quantity);


        CartItem? item4 = currentCart.Items.FirstOrDefault(i => i.ProductId == product4.Id);
        Assert.NotNull(item4);
        Assert.Equal(finalP4Qty, item4.Quantity);
    }


    [Fact]
    public async Task Add_ProductNotFound()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        Cart newCart = Cart.Create(user.Id);

        context.Add(newCart);
        context.SaveChanges();

        CartRepository cartRep = CategoryFactory.CreateCartRepository(context);
        //Act
        Result result = await cartRep.AddAsync(user.Id, int.MaxValue, 1, CancellationToken.None);
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CartError.ProductNotfound, result.Error);
    }

    [Fact]
    public async Task DecreaseDecreaseLimit()
    {
        using var context = ContextHelper.Create();
        CartRepository cartRep = CategoryFactory.CreateCartRepository(context);
        User user = ContextHelper.CreateUser(context);
        Product product1 = ProductFactory.Create(context, 1000);

        Cart currentCart = new()
        {
            UserId = user.Id,
            Status = CartStatus.Active,
            Items = [
                new CartItem{
                    ProductId = product1.Id,
                    Quantity = 1,
                    Item = product1
                }
            ]
        };

        context.Add(currentCart);
        context.SaveChanges();
        context.ChangeTracker.Clear();
        
        //Act
        Result result = await cartRep.DecreaseAsync(user.Id, product1.Id);
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CartError.DecreaseLimit, result.Error);
    }


    [Theory]
    [InlineData(10_000.0, 2, 20_000)]
    [InlineData( 5_000, 2, 10_000)]
    public async Task Decrease_ProductNotAvailable(decimal product1Price, int qty, decimal total)
    {
        if (total != (product1Price * qty))
            throw new ArgumentException("Invalid inline data");

        using var context = ContextHelper.Create();
        CartRepository cartRep = CategoryFactory.CreateCartRepository(context);
        User user = ContextHelper.CreateUser(context);
        Product product1 = ProductFactory.Create(context, product1Price);
        Product product2 = ProductFactory.Create(context, 1000);

        Cart activeCart = new()
        {
            UserId = user.Id,
            Status = CartStatus.Active,
            Items = [
                new CartItem{
                    ProductId = product1.Id,
                    Quantity = qty,
                    Item = product1
                },
                new CartItem{
                    ProductId = product1.Id,
                    Quantity = 2,
                    Item = product2
                },
            ]
        };
        product2.IsActive = false;
        context.Update(product2);
        context.Add(activeCart);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        //Act
        Result result = await cartRep.DecreaseAsync(user.Id, product2.Id);
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CartError.ItemNotfound, result.Error);
        Assert.Equal(total, activeCart.Total);
    }


    [Theory]
    [InlineData(10_000.0, 2, 500.0, 6, 20_000.0)]
    [InlineData(10.0, 10, 5.0, 10, 100.0)]
    [InlineData(25_000.0, 2, 10_000.0, 6, 50_000.0)]
    public async Task GetCart_WhenSomeProductInCartIsInactive(decimal Product1Price, int QtyP1, decimal Product4Price, int QtyP4, decimal Total)
    {
        using var context = ContextHelper.Create();
        CartRepository cartRep = CategoryFactory.CreateCartRepository(context);
        User user = ContextHelper.CreateUser(context);
        Product product1 = ProductFactory.Create(context, Product1Price);
        Product product2 = ProductFactory.Create(context, 3000);
        Product product3 = ProductFactory.Create(context, 7000);
        Product product4 = ProductFactory.Create(context, Product4Price);

        Cart oldCart = new()
        {
            UserId = user.Id,
            Status = CartStatus.Converted,
            Items = [
                new CartItem {
                    ProductId = product1.Id,
                    Quantity = 383,
                    Item = product1
                }
            ]
        };
        Cart activeCart = Cart.Create(user.Id);

        context.AddRange([activeCart, oldCart]);
        context.SaveChanges();



        //Act
        Result result1 = await cartRep.AddAsync(user.Id, product1.Id, QtyP1, CancellationToken.None);
        Assert.True(result1.IsSuccess);
        Result result2 = await cartRep.AddAsync(user.Id, product4.Id, QtyP4, CancellationToken.None);
        Assert.True(result2.IsSuccess);
        //Inactive product behind Get-cart operation
        product4.IsActive = false;
        context.Update(product4);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var result = await cartRep.GetAsync(user.Id, CancellationToken.None);
        Cart? userCart = result.Value;
        Assert.True(result.IsSuccess);
        Assert.NotNull(userCart);
        Assert.Single(userCart.Items);
        int totalProducts = QtyP1;
        Assert.Equal(totalProducts, userCart.Items.Sum(i => i.Quantity));
        Assert.Contains(userCart.Items, i => i.ProductId == product1.Id);
        Assert.DoesNotContain(userCart.Items, i => i.ProductId == product4.Id);
        Assert.Equal(Total, userCart.Total);
    }

    public static TheoryData<decimal, int, int, decimal, int, int, decimal>
    ProductsInCartCasesForDecreaseTest =>
    new()
    {
        //P1 price   Qty    DecQty   P2 Price   Qty   DecQty   Total
        {10_000.0m,    2,   1,         500.0m,    6,     1,    12_500.0m},
        {10.0m,       10,   3,          50.0m,   10,     5,       320.0m},
        {25_000.0m,    4,   3,      10_000.0m,    6,     1,    75_000.0m},
        {5_000.0m,     4,   3,      10_000.0m,    6,     1,    55_000.0m},
    };

}