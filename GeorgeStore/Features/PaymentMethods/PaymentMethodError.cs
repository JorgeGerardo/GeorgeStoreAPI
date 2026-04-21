using GeorgeStore.Common;

namespace GeorgeStore.Features.PaymentMethods;

public static class PaymentMethodError
{
    public static readonly Error UnexpectedError =
        new("Has ocurred an error", "Try again in some minutes", "PaymentMethod.UnexpectedError");

    public static readonly Error NotFound =
        new("Payment method not found", "Try with another", "PaymentMethod.NotFound");

    public static readonly Error InvalidCardNumber =
        new("Invalid Card number", "Card number must be 16 digits", "PaymentMethod.InvalidCardNumber");

    public static readonly Error InvalidExpYear =
        new("Range out", "The expiration year is out of range", "PaymentMethod.InvalidExpYear");
}