using Bogus;
using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using Moq;

namespace GeorgeStore.Tests.Factories;

internal static class AddressFactory
{
    public static AddressRepository CreateRepository(GeorgeStoreContext context)
    {
        var dbConnection = new Mock<IDbConnectionFactory>();
        return new(context, dbConnection.Object);
    }

    public static AddressCreateDto CreateRandomAddressDto(bool isDefault = false, string? alias = null)
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

    public static Address CreateRandomAddress(GeorgeStoreContext context, User user, bool isDefault = false)
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
