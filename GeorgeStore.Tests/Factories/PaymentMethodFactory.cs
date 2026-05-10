using GeorgeStore.Features.PaymentMethods;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Features.Users;
using Moq;

namespace GeorgeStore.Tests.Factories;

internal static class PaymentMethodFactory
{
    public static PaymentMethodRepository CreatePaymentMethodRepository(GeorgeStoreContext context)
    {
        var conn = new Mock<IDbConnectionFactory>();
        return new PaymentMethodRepository(context, conn.Object);
    }

    public static PaymentMethod CreateRandomPaymentMethod(GeorgeStoreContext context, User user, bool IsDefault = false)
    {
        var newPaymentMethod = new PaymentMethod
        {
            User = user,
            Brand = "Visa",
            Token = Guid.NewGuid().ToString(),
            LastDigits = "1234",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpYear = 2039,
            ExpMonth = 1,
            CardHolderName = "Jorguito Lopez",
            IsDefault = IsDefault,
        };

        context.Add(newPaymentMethod);
        context.SaveChanges();
        return newPaymentMethod;
    }

}
