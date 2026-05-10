using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using GeorgeStore.Common.Shared;
using Moq;

namespace GeorgeStore.Tests.AddressTests;

public class AddressRepositoryTest
{
    [Fact]
    public async Task SetAsDefaultTest()
    {
        using var context = ContextHelper.Create();
        var dbConnection = new Mock<IDbConnectionFactory>();
        User user = CreateUser(context);
        AddressRepository addressRepository = new(context, dbConnection.Object);

        var workAdds = Address.Create(user.Id, "Work", "", "", "", "", "", "", "", "", true);
        var homeAdds = Address.Create(user.Id, "Home", "", "", "", "", "", "", "", "", false);
        var MomHomAdds = Address.Create(user.Id, "Mom's house", "", "", "", "", "", "", "", "", false);

        user.Addresses = [workAdds, homeAdds, MomHomAdds];

        context.Users.Update(user);
        context.SaveChanges();

        Result result = await addressRepository.SetAsDefaultAsync(user.Id, homeAdds.Id);

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
        using var context = ContextHelper.Create();
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
        Result result = await addressRepository.SetAsDefaultAsync(user.Id, homeAdds.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(AddressError.NotFound, result.Error);
    }

    [Fact]
    public async Task RemoveTest()
    {
        using var context = ContextHelper.Create();
        var dbConnection = new Mock<IDbConnectionFactory>();
        User user = CreateUser(context);
        AddressRepository addressRepository = new(context, dbConnection.Object);

        var workAdds = Address.Create(user.Id, "Work", "", "", "", "", "", "", "", "", true);
        var homeAdds = Address.Create(user.Id, "Home", "", "", "", "", "", "", "", "", false);
        var MomHomAdds = Address.Create(user.Id, "Mom's house", "", "", "", "", "", "", "", "", false);

        user.Addresses = [workAdds, homeAdds, MomHomAdds];

        context.Users.Update(user);
        context.SaveChanges();

        Result result = await addressRepository.RemoveAsync(user.Id, homeAdds.Id);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, user.Addresses.Count);
        Assert.DoesNotContain(user.Addresses, a => a.Id == homeAdds.Id);

    }

    [Fact]
    public async Task Remove_AddressNotFound()
    {
        using var context = ContextHelper.Create();
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
        Result result = await addressRepository.RemoveAsync(user.Id, homeAdds.Id);
        Assert.False(result.IsSuccess);
        Assert.Equal(AddressError.NotFound, result.Error);

    }

    [Fact]
    public async Task Add()
    {
        using var context = ContextHelper.Create();
        var dbConnection = new Mock<IDbConnectionFactory>();
        User user = CreateUser(context);
        AddressRepository addressRepository = new(context, dbConnection.Object);
        const string defaultAddressAlias = "Work";
        AddressCreateDto request = new("Home", "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", false);
        AddressCreateDto request2 = new(defaultAddressAlias, "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", true);

        //Act
        Result result1 = await addressRepository.AddAsync(user.Id, request);
        Result result2 = await addressRepository.AddAsync(user.Id, request2);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal(2, user.Addresses.Count);
        Assert.Single(user.Addresses, a => a.IsDefault);
        Address? defaultAddress = user.Addresses.FirstOrDefault(a => a.IsDefault);
        Assert.NotNull(defaultAddress);
        Assert.Equal(defaultAddressAlias, defaultAddress.Alias);
    }

    [Fact]
    public async Task Add_WithoutDefaultAddress()
    {
        using var context = ContextHelper.Create();
        var dbConnection = new Mock<IDbConnectionFactory>();
        User user = CreateUser(context);
        AddressRepository addressRepository = new(context, dbConnection.Object);
        const string defaultAddressAlias = "Work";
        AddressCreateDto request = new("Home", "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", false);
        AddressCreateDto request2 = new(defaultAddressAlias, "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", false);

        //Act
        Result result1 = await addressRepository.AddAsync(user.Id, request);
        Result result2 = await addressRepository.AddAsync(user.Id, request2);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal(2, user.Addresses.Count);
        Address? defaultAddress = user.Addresses.FirstOrDefault(a => a.IsDefault);
        Assert.Null(defaultAddress);
    }

    [Fact]
    public async Task Add_LimitedReached()
    {
        using var context = ContextHelper.Create();
        var dbConnection = new Mock<IDbConnectionFactory>();
        User user = CreateUser(context);
        AddressRepository addressRepository = new(context, dbConnection.Object);
        const string defaultAddressAlias = "Work";
        AddressCreateDto request1 = new("Home", "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", true);
        AddressCreateDto request2 = new("Home2", "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", true);
        AddressCreateDto request3 = new(defaultAddressAlias, "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", true);
        AddressCreateDto request4 = new("Forest", "Street", "123", "Los Mochis", "Sinaloa", "81200", "MX", "Jorge", "6681234567", true);

        //Act
        Result result1 = await addressRepository.AddAsync(user.Id, request1);
        Result result2 = await addressRepository.AddAsync(user.Id, request2);
        Result result3 = await addressRepository.AddAsync(user.Id, request3);
        Result result4 = await addressRepository.AddAsync(user.Id, request4);

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.True(result3.IsSuccess);

        Assert.False(result4.IsSuccess);
        Assert.Equal(AddressError.LimitReached, result4.Error);

        Assert.Equal(3, user.Addresses.Count);
        Assert.Single(user.Addresses, a => a.IsDefault);
        Address? defaultAddress = user.Addresses.FirstOrDefault(a => a.IsDefault);
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
