using GeorgeStore.Common;

namespace GeorgeStore.Features.PaymentMethods;

public static class PaymentMethodError
{
    public static readonly Error NotFound =
        new("Payment method not found", "Try with another", "PaymentMethod.NotFound", ErrorType.NotFound);

    public static readonly Error InvalidCardNumber =
        new("Invalid Card number", "Card number must be 16 digits", "PaymentMethod.InvalidCardNumber", ErrorType.Validation);

    public static readonly Error InvalidExpYear =
        new("Range out", "The expiration year is out of range", "PaymentMethod.InvalidExpYear", ErrorType.Validation);

    public static readonly Error PaymentMethodLimitReached =
        new("Limit reached", "Payment-methods limit reached", "PaymentMethod.PaymentMethodLimitReached", ErrorType.Conflict);
}