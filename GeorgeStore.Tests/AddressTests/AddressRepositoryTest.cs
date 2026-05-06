using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using Moq;

namespace GeorgeStore.Tests.AddressTests;

public class AddressRepositoryTest
{
    [Fact]
    public async Task SetAsDefaultTest()
    {
        var context = ContextHelper.CreateContext();
        var dbConnection = new Mock<IDbConnectionFactory>();
        User user = CreateUser(context);
        AddressRepository addressRepository = new(context, dbConnection.Object);

        var workAdds = Address.Create(user.Id, "Work", "", "", "", "", "", "", "", "", true);
        var homeAdds = Address.Create(user.Id, "Home", "", "", "", "", "", "", "", "", false);
        var MomHomAdds = Address.Create(user.Id, "Mom's house", "", "", "", "", "", "", "", "", false);

        user.Addresses = [workAdds, homeAdds, MomHomAdds];

        context.Users.Update(user);
        context.SaveChanges();

        var result = await addressRepository.SetAsDefaultAsync(user.Id, homeAdds.Id);

        //Act
        Assert.True(result.IsSuccess);
        Assert.Single(user.Addresses, a => a.IsDefault);
        Assert.False(workAdds.IsDefault);
        Assert.False(MomHomAdds.IsDefault);
        Assert.True(homeAdds.IsDefault);
    }

    [Fact]
    public async Task SetAsDefaultTest_AddressNotFound()
    {
        var context = ContextHelper.CreateContext();
        var dbConnection = new Mock<IDbConnectionFactory>();
        User user = CreateUser(context);
        AddressRepository addressRepository = new(context, dbConnection.Object);

        var workAdds = Address.Create(user.Id, "Work", "", "", "", "", "", "", "", "", true);
        var homeAdds = Address.Create(user.Id, "Home", "", "", "", "", "", "", "", "", false);

        user.Addresses = [workAdds, homeAdds];

        context.Users.Update(user);
        context.SaveChanges();


        //Act
        context.Addresses.Remove(homeAdds);
        context.SaveChanges();
        var result = await addressRepository.SetAsDefaultAsync(user.Id, homeAdds.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(AddressError.NotFound, result.Error);
    }

    [Fact]
    public async Task RemoveTest()
    {
        var context = ContextHelper.CreateContext();
        var dbConnection = new Mock<IDbConnectionFactory>();
        User user = CreateUser(context);
        AddressRepository addressRepository = new(context, dbConnection.Object);

        var workAdds = Address.Create(user.Id, "Work", "", "", "", "", "", "", "", "", true);
        var homeAdds = Address.Create(user.Id, "Home", "", "", "", "", "", "", "", "", false);
        var MomHomAdds = Address.Create(user.Id, "Mom's house", "", "", "", "", "", "", "", "", false);

        user.Addresses = [workAdds, homeAdds, MomHomAdds];

        context.Users.Update(user);
        context.SaveChanges();

        var result = await addressRepository.RemoveAsync(user.Id, homeAdds.Id);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, user.Addresses.Count);
        Assert.DoesNotContain(user.Addresses, a => a.Id == homeAdds.Id);

    }

    [Fact]
    public async Task Remove_AddressNotFound()
    {
        var context = ContextHelper.CreateContext();
        var dbConnection = new Mock<IDbConnectionFactory>();
        User user = CreateUser(context);
        AddressRepository addressRepository = new(context, dbConnection.Object);

        var homeAdds = Address.Create(user.Id, "Home", "", "", "", "", "", "", "", "", false);
        var MomHomAdds = Address.Create(user.Id, "Mom's house", "", "", "", "", "", "", "", "", false);

        user.Addresses = [homeAdds, MomHomAdds];

        context.Users.Update(user);
        context.SaveChanges();

        //Act
        user.Addresses.Remove(homeAdds);
        context.SaveChanges();
        var result = await addressRepository.RemoveAsync(user.Id, homeAdds.Id);
        Assert.False(result.IsSuccess);
        Assert.Equal(AddressError.NotFound, result.Error);

    }

    [Fact]
    public async Task Add()
    {
        var context = ContextHelper.CreateContext();
        var dbConnection = new Mock<IDbConnectionFactory>();
        User user = CreateUser(context);
        AddressRepository addressRepository = new(context, dbConnection.Object);
        const string defaultAddressAlias = "Work";
        AddressCreateDto request = new("Home", "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", false);
        AddressCreateDto request2 = new(defaultAddressAlias, "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", true);

        //Act
        var result1 = await addressRepository.AddAsync(user.Id, request);
        var result2 = await addressRepository.AddAsync(user.Id, request2);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal(2, user.Addresses.Count);
        Assert.Single(user.Addresses, a => a.IsDefault);
        var defaultAddress = user.Addresses.FirstOrDefault(a => a.IsDefault);
        Assert.NotNull(defaultAddress);
        Assert.Equal(defaultAddressAlias, defaultAddress.Alias);
    }

    [Fact]
    public async Task Add_WithoutDefaultAddress()
    {
        var context = ContextHelper.CreateContext();
        var dbConnection = new Mock<IDbConnectionFactory>();
        User user = CreateUser(context);
        AddressRepository addressRepository = new(context, dbConnection.Object);
        const string defaultAddressAlias = "Work";
        AddressCreateDto request = new("Home", "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", false);
        AddressCreateDto request2 = new(defaultAddressAlias, "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", false);

        //Act
        var result1 = await addressRepository.AddAsync(user.Id, request);
        var result2 = await addressRepository.AddAsync(user.Id, request2);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal(2, user.Addresses.Count);
        var defaultAddress = user.Addresses.FirstOrDefault(a => a.IsDefault);
        Assert.Null(defaultAddress);
    }

    [Fact]
    public async Task Add_LimitedReached()
    {
        var context = ContextHelper.CreateContext();
        var dbConnection = new Mock<IDbConnectionFactory>();
        User user = CreateUser(context);
        AddressRepository addressRepository = new(context, dbConnection.Object);
        const string defaultAddressAlias = "Work";
        AddressCreateDto request1 = new("Home", "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", true);
        AddressCreateDto request2 = new("Home2", "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", true);
        AddressCreateDto request3 = new(defaultAddressAlias, "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", true);//No debería dar true el test
        AddressCreateDto request4 = new("Forest", "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", true);

        //Act
        var result1 = await addressRepository.AddAsync(user.Id, request1);
        var result2 = await addressRepository.AddAsync(user.Id, request2);
        var result3 = await addressRepository.AddAsync(user.Id, request3);
        var result4 = await addressRepository.AddAsync(user.Id, request4);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.True(result3.IsSuccess);

        Assert.False(result4.IsSuccess);
        Assert.Equal(AddressError.LimitReached, result4.Error);

        Assert.Equal(3, user.Addresses.Count);
        Assert.Single(user.Addresses, a => a.IsDefault);
        var defaultAddress = user.Addresses.FirstOrDefault(a => a.IsDefault);
        Assert.NotNull(defaultAddress);
        Assert.Equal(defaultAddressAlias, defaultAddress.Alias);
    }


    //Common arranges
    private static User CreateUser(GeorgeStoreContext context)
    {
        User user = new("Jorguito", "jorguito@gmail.com");
        context.Add(user);
        context.SaveChanges();
        return user;
    }
}
