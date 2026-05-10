using GeorgeStore.Features.Categories;
using GeorgeStore.Features.Products;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using GeorgeStore.Common.Shared;
using Moq;
using GeorgeStore.Tests.Factories;

namespace GeorgeStore.Tests.Products;

public class ProductRepositoryTests
{
    [Fact]
    public async Task CreateTest()
    {
        using var context = ContextHelper.Create();
        ProductRepository productRep = ProductFactory.CreateRepository(context);
        ProductCreateDTO request = new("Pixel 10", 3200m, "", "", 1);

        //Act
        Result result = await productRep.CreateAsync(request);
        //Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetByIdTest()
    {
        using var context = ContextHelper.Create();
        ProductRepository productRep = ProductFactory.CreateRepository(context);
        Category category = ProductFactory.CreateRandomCategory(context);
        Product product1 = Product.Create("LaptopAsus", "description", category.Id, "", 1000, true);
        context.Products.Add(product1);
        context.SaveChanges();

        //Act
        var result = await productRep.GetByIdAsync(product1.Id);
        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(product1.Name, result.Value.Name);
    }

    [Fact]
    public async Task GetById_ProductNoActiveTest()
    {
        using var context = ContextHelper.Create();
        var connFactory = new Mock<IDbConnectionFactory>();
        Category category = ProductFactory.CreateRandomCategory(context);
        Product product1 = Product.Create("LaptopAsus", "description", category.Id, "", 1000, false);
        context.Products.Add(product1);
        context.SaveChanges();

        //Act
        ProductRepository productRep = new(connFactory.Object, context);
        var result = await productRep.GetByIdAsync(product1.Id);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.Notfound, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public async Task Exist_False_and_True_Test()
    {
        using var context = ContextHelper.Create();
        ProductRepository productRep = ProductFactory.CreateRepository(context);
        Category category = ProductFactory.CreateRandomCategory(context);
        Product product1 = Product.Create("LaptopAsus", "description", category.Id, "", 1000, true);
        Product product2 = Product.Create("Galaxy S26", "description", category.Id, "", 3000, false);
        context.Products.AddRange([product1, product2]);
        context.SaveChanges();

        //Act
        bool result1 = await productRep.ExistAsync(product1.Id);
        bool result2 = await productRep.ExistAsync(product2.Id);

        //Assert
        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    public async Task RemoveTest()
    {
        using var context = ContextHelper.Create();
        ProductRepository productRep = ProductFactory.CreateRepository(context);
        Category category = ProductFactory.CreateRandomCategory(context);
        Product product1 = Product.Create("LaptopAsus", "description", category.Id, "", 1000, false);
        Product product2 = Product.Create("Galaxy S26", "description", category.Id, "", 3000, true);
        context.Products.AddRange([product1, product2]);
        context.SaveChanges();


        //Act
        var result1 = await productRep.RemoveAsync(product1.Id);
        var result2 = await productRep.RemoveAsync(product2.Id);

        //Assert
        var productSearch1 = await productRep.GetByIdAsync(product1.Id);
        Assert.False(productSearch1.IsSuccess);
        Assert.Equal(ProductError.Notfound, productSearch1.Error);
        Assert.True(result1.IsSuccess);

        var productSearch2 = await productRep.GetByIdAsync(product2.Id);
        Assert.False(productSearch2.IsSuccess);
        Assert.Equal(ProductError.Notfound, productSearch2.Error);
        Assert.True(result2.IsSuccess);
    }

    [Fact]
    public async Task RemoveTest_NotFound()
    {
        using var context = ContextHelper.Create();
        ProductRepository productRep = ProductFactory.CreateRepository(context);

        //Act
        Result result = await productRep.RemoveAsync(int.MaxValue);
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.Notfound, result.Error);
    }

}
