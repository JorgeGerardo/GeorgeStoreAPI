using Bogus;
using GeorgeStore.Features.PaymentMethods;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using System.Data;

namespace GeorgeStore.Tests.Factories;

internal static class PaymentMethodFactory
{
    public static PaymentMethodRepository CreateRepository(GeorgeStoreContext context)
    {
        IDbConnection sqlConn = ContextHelper.CreateSqlConn(context);
        var connFactory = ContextHelper.CreateConnectionFactory(sqlConn);
        return new PaymentMethodRepository(context, connFactory.Object);
    }

    public static PaymentMethod Create(GeorgeStoreContext context, User user, bool IsDefault = false)
    {
        Faker faker = new("es_MX");
        var newPaymentMethod = new PaymentMethod
        {
            User = user,
            Brand = faker.PickRandom("Visa", "Mastercard", "American Express"),
            Token = Guid.NewGuid().ToString(),
            LastDigits = faker.Random.ReplaceNumbers("####"),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpYear = faker.Random.Number(2030, 2080),
            ExpMonth = faker.Random.Number(1, 12),
            CardHolderName = "Jorguito Lopez",
            IsDefault = IsDefault,
        };

        context.Add(newPaymentMethod);
        context.SaveChanges();
        return newPaymentMethod;
    }

}
