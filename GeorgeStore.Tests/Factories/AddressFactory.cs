using Bogus;
using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using System.Data;

namespace GeorgeStore.Tests.Factories;

internal static class AddressFactory
{
    public static AddressRepository CreateRepository(GeorgeStoreContext context)
    {
        IDbConnection sqlConn = ContextHelper.CreateSqlConn(context);
        var connFactory = ContextHelper.CreateConnectionFactory(sqlConn);
        return new(context, connFactory.Object);
    }

    public static AddressCreateDto CreateRandomAddressDto(bool isDefault = false, string? alias = null)
    {
        Faker faker = new("es_MX");

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

    public static Address CreateRandom(GeorgeStoreContext context, User user, bool isDefault = false)
    {
        Faker faker = new("es_MX");
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
