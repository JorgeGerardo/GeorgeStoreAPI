using Bogus;
using GeorgeStore.Common.Shared;
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
        using var context = ContextHelper.Create();
        AddressRepository addressRepository = CreateRepository(context);
        User user = ContextHelper.CreateUser(context);
        Address addr1 = CreateRandomAddress(context, user, true);
        Address addr2 = CreateRandomAddress(context, user, false);
        Address addr3 = CreateRandomAddress(context, user, false);

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
        AddressRepository addressRepository = CreateRepository(context);
        Address addr1 = CreateRandomAddress(context, user, true);
        Address addr2 = CreateRandomAddress(context, user, false);

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
        AddressRepository addressRepository = CreateRepository(context);

        var addr1 = CreateRandomAddress(context, user, true);
        var addr2 = CreateRandomAddress(context, user, false);
        var addr3 = CreateRandomAddress(context, user, false);

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
        AddressRepository addressRepository = CreateRepository(context);
        Address addr1 = CreateRandomAddress(context, user, false);
        Address addr2 = CreateRandomAddress(context, user, false);
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
        AddressRepository addressRepository = CreateRepository(context);
        const string defaultAddressAlias = "Work";
        AddressCreateDto request1 = CreateRandomAddressDto(isDefault: false);
        AddressCreateDto request2 = CreateRandomAddressDto(isDefault: true, alias: defaultAddressAlias);

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
        AddressRepository addressRepository = CreateRepository(context);
        AddressCreateDto request1 = CreateRandomAddressDto(isDefault: false);
        AddressCreateDto request2 = CreateRandomAddressDto(isDefault: false);

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
        AddressRepository addressRepository = CreateRepository(context);
        const string defaultAddressAlias = "Work";
        AddressCreateDto request1 = CreateRandomAddressDto(isDefault: true);
        AddressCreateDto request2 = CreateRandomAddressDto(isDefault: true);
        AddressCreateDto request3 = CreateRandomAddressDto(isDefault: true, alias:  defaultAddressAlias);
        AddressCreateDto request4 = CreateRandomAddressDto(isDefault: true);

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


    //Common arranges
    private static AddressRepository CreateRepository(GeorgeStoreContext context)
    {
        var dbConnection = new Mock<IDbConnectionFactory>();
        return new(context, dbConnection.Object);
    }

    private static AddressCreateDto CreateRandomAddressDto(bool isDefault = false, string? alias = null)
    {
        var faker = new Faker("es_MX");

        return new AddressCreateDto(
            alias is null 
                ? faker.PickRandom("Home", "Work", "Mom's House", "Office")
                : alias,
            faker.Address.StreetAddress(),
            faker.Address.County(),
            faker.Address.City(),
            faker.Address.State(),
            faker.Address.ZipCode(),
            faker.Address.BuildingNumber(),
            faker.Random.Bool()
                ? faker.Random.Number(1, 20).ToString()
                : null,
            faker.Lorem.Sentence(),
            isDefault
        );
    }
    private static Address CreateRandomAddress(GeorgeStoreContext context, User user, bool isDefault = false)
    {
        var faker = new Faker("es_MX");
        Address newAddr = Address.Create(
            user.Id, 
            faker.Address.StreetName(), 
            faker.Address.StreetAddress(),
            faker.Address.County(),
            faker.Address.City(), 
            faker.Address.State(),
            faker.Address.ZipCode(),
            faker.Address.BuildingNumber(),
            faker.Random.Bool() ? faker.Random.Number(1, 20).ToString() : null,
            faker.Lorem.Sentence(),
            isDefault
        );
        context.Add(newAddr);
        context.SaveChanges();
        return newAddr;
    }
}
