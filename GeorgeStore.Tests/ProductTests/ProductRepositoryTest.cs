using GeorgeStore.Features.Categories;
using GeorgeStore.Features.Products;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using Moq;

namespace GeorgeStore.Tests.ProductTests;

public class ProductRepositoryTest
{
    [Fact]
    public async Task CreateTest()
    {
        using var context = ContextHelper.Create();
        var connFactory = new Mock<IDbConnectionFactory>();

        ProductCreateDTO request = new("Pixel 10", 3200m, "", "", 1);
        ProductRepository productRep = new(connFactory.Object, context);

        //Act
        var result = await productRep.CreateAsync(request);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetByIdTest()
    {
        using var context = ContextHelper.Create();
        var connFactory = new Mock<IDbConnectionFactory>();
        Category category1 = new() { Name = "Electronic", Image = "" };
        Product product1 = Product.Create("LaptopAsus", "description", 1, "", 1000, true);
        Product product2 = Product.Create("Galaxy S26", "description", 1, "", 3000, true);
        Product product3 = Product.Create("Pixel 10XL", "description", 1, "", 7000, true);
        Product product4 = Product.Create("Huawei p60", "description", 1, "", 8000, true);

        context.Categories.Add(category1);
        context.Products.AddRange([product1, product2, product3, product4]);
        context.SaveChanges();
        ProductRepository productRep = new(connFactory.Object, context);


        //Act
        var result = await productRep.GetByIdAsync(product1.Id);
        Assert.True(result.IsSuccess);
        Assert.Equal(product1.Name, result.Value.Name);
    }

    [Fact]
    public async Task GetById_ProductNoActiveTest()
    {
        using var context = ContextHelper.Create();
        var connFactory = new Mock<IDbConnectionFactory>();
        Category category1 = new() { Name = "Electronic", Image = "" };
        Product product1 = Product.Create("LaptopAsus", "description", 1, "", 1000, false);

        context.Categories.Add(category1);
        context.Products.Add(product1);
        context.SaveChanges();


        //Act
        ProductRepository productRep = new(connFactory.Object, context);
        var result = await productRep.GetByIdAsync(product1.Id);
        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.Notfound, result.Error);
    }

    [Fact]
    public async Task Exist_False_and_True_Test()
    {
        using var context = ContextHelper.Create();
        var connFactory = new Mock<IDbConnectionFactory>();
        Category category1 = new() { Name = "Electronic", Image = "" };
        Product product1 = Product.Create("LaptopAsus", "description", 1, "", 1000, true);
        Product product2 = Product.Create("Galaxy S26", "description", 1, "", 3000, false);

        context.Categories.Add(category1);
        context.Products.AddRange([product1, product2]);
        context.SaveChanges();


        //Act
        ProductRepository productRep = new(connFactory.Object, context);
        var result1 = await productRep.ExistAsync(product1.Id);
        var result2 = await productRep.ExistAsync(product2.Id);
        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    public async Task RemoveTest()
    {
        using var context = ContextHelper.Create();
        var connFactory = new Mock<IDbConnectionFactory>();
        Category category1 = new() { Name = "Electronic", Image = "" };
        Product product1 = Product.Create("LaptopAsus", "description", 1, "", 1000, false);
        Product product2 = Product.Create("Galaxy S26", "description", 1, "", 3000, true);

        context.Categories.Add(category1);
        context.Products.AddRange([product1, product2]);
        context.SaveChanges();


        //Act
        ProductRepository productRep = new(connFactory.Object, context);
        var result1 = await productRep.RemoveAsync(product1.Id);
        var result2 = await productRep.RemoveAsync(product2.Id);

        var productSearch1 = await productRep.GetByIdAsync(product1.Id);
        Assert.False(productSearch1.IsSuccess);
        Assert.Equal(ProductError.Notfound, productSearch1.Error);

        var productSearch2 = await productRep.GetByIdAsync(product2.Id);
        Assert.False(productSearch2.IsSuccess);
        Assert.Equal(ProductError.Notfound, productSearch2.Error);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
    }

    [Fact]
    public async Task RemoveTest_NotFound()
    {
        using var context = ContextHelper.Create();
        var connFactory = new Mock<IDbConnectionFactory>();


        //Act
        ProductRepository productRep = new(connFactory.Object, context);
        var result = await productRep.RemoveAsync(1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ProductError.Notfound, result.Error);
    }
}
