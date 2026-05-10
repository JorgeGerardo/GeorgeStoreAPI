using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Users;
using GeorgeStore.Tests.Common;
using GeorgeStore.Tests.Factories;

namespace GeorgeStore.Tests.Addresses;

public class AddressRepositoryTests
{
    [Fact]
    public async Task SetAsDefaultTest()
    {
        using var context = ContextHelper.Create();
        AddressRepository addressRepository = AddressFactory.CreateRepository(context);
        User user = ContextHelper.CreateUser(context);
        Address addr1 = AddressFactory.CreateRandomAddress(context, user, true);
        Address addr2 = AddressFactory.CreateRandomAddress(context, user, false);
        Address addr3 = AddressFactory.CreateRandomAddress(context, user, false);

        //Act
        Result result = await addressRepository.SetAsDefaultAsync(user.Id, addr2.Id);
        //Assert
        Assert.True(result.IsSuccess);
        Address defaultAddress = Assert.Single(user.Addresses, a => a.IsDefault);
        Assert.Equal(defaultAddress.Id, addr2.Id);
    }

    [Fact]
    public async Task SetAsDefaultTest_AddressNotFound()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        AddressRepository addressRepository = AddressFactory.CreateRepository(context);
        Address addr1 =  AddressFactory.CreateRandomAddress(context, user, true);
        Address addr2 = AddressFactory.CreateRandomAddress(context, user, false);

        //Act
        context.Addresses.Remove(addr2);
        context.SaveChanges();
        Result result = await addressRepository.SetAsDefaultAsync(user.Id, addr2.Id);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AddressError.NotFound, result.Error);
    }

    [Fact]
    public async Task RemoveTest()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        AddressRepository addressRepository = AddressFactory.CreateRepository(context);

        var addr1 = AddressFactory.CreateRandomAddress(context, user, true);
        var addr2 = AddressFactory.CreateRandomAddress(context, user, false);
        var addr3 = AddressFactory.CreateRandomAddress(context, user, false);

        Result result = await addressRepository.RemoveAsync(user.Id, addr2.Id);
        Assert.True(result.IsSuccess);
        Assert.Equal(2, user.Addresses.Count);
        Assert.DoesNotContain(user.Addresses, a => a.Id == addr2.Id);

    }

    [Fact]
    public async Task Remove_AddressNotFound()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        AddressRepository addressRepository = AddressFactory.CreateRepository(context);
        Address addr1 = AddressFactory.CreateRandomAddress(context, user, false);
        Address addr2 = AddressFactory.CreateRandomAddress(context, user, false);
        user.Addresses.Remove(addr1);
        context.SaveChanges();

        //Act
        Result result = await addressRepository.RemoveAsync(user.Id, addr1.Id);
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(AddressError.NotFound, result.Error);

    }

    [Fact]
    public async Task Add()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        AddressRepository addressRepository = AddressFactory.CreateRepository(context);
        const string defaultAddressAlias = "Work";
        AddressCreateDto request1 = AddressFactory.CreateRandomAddressDto(isDefault: false);
        AddressCreateDto request2 = AddressFactory.CreateRandomAddressDto(isDefault: true, alias: defaultAddressAlias);

        //Act
        Result result1 = await addressRepository.AddAsync(user.Id, request1);
        Result result2 = await addressRepository.AddAsync(user.Id, request2);
        //Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal(2, user.Addresses.Count);
        Address defaultAddress = Assert.Single(user.Addresses, a => a.IsDefault);
        Assert.Equal(defaultAddressAlias, defaultAddress.Alias);
    }

    [Fact]
    public async Task Add_NoOneIsDefault()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        AddressRepository addressRepository = AddressFactory.CreateRepository(context);
        AddressCreateDto request1 = AddressFactory.CreateRandomAddressDto(isDefault: false);
        AddressCreateDto request2 = AddressFactory.CreateRandomAddressDto(isDefault: false);

        //Act
        Result result1 = await addressRepository.AddAsync(user.Id, request1);
        Result result2 = await addressRepository.AddAsync(user.Id, request2);
        //Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal(2, user.Addresses.Count);
        Assert.DoesNotContain(user.Addresses, a => a.IsDefault);
    }

    [Fact]
    public async Task Add_LimitedReached()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        AddressRepository addressRepository = AddressFactory.CreateRepository(context);
        const string defaultAddressAlias = "Work";
        AddressCreateDto request1 = AddressFactory.CreateRandomAddressDto(isDefault: true);
        AddressCreateDto request2 = AddressFactory.CreateRandomAddressDto(isDefault: true);
        AddressCreateDto request3 = AddressFactory.CreateRandomAddressDto(isDefault: true, alias:  defaultAddressAlias);
        AddressCreateDto request4 = AddressFactory.CreateRandomAddressDto(isDefault: true);

        //Act
        Result result1 = await addressRepository.AddAsync(user.Id, request1);
        Result result2 = await addressRepository.AddAsync(user.Id, request2);
        Result result3 = await addressRepository.AddAsync(user.Id, request3);
        Result result4 = await addressRepository.AddAsync(user.Id, request4);

        //Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.True(result3.IsSuccess);

        Assert.False(result4.IsSuccess);
        Assert.Equal(AddressError.LimitReached, result4.Error);

        Assert.Equal(3, user.Addresses.Count);
        Address defaultAddress = Assert.Single(user.Addresses, a => a.IsDefault);
        Assert.NotNull(defaultAddress);
        Assert.Equal(defaultAddressAlias, defaultAddress.Alias);
    }
}
