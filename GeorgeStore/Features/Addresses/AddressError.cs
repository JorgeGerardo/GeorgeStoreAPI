using GeorgeStore.Common.Shared;

namespace GeorgeStore.Features.Addresses;

public static class AddressError
{
    public static readonly Error Conflict =
                new("Error saving information", "Try again in some minutes", "Address.Conflict", ErrorType.Conflict);

    public static readonly Error NotFound =
                new("Address not found", "This address isn't exist", "Address.NotFound", ErrorType.NotFound);

    public static readonly Error LimitReached =
                new("Users only can register 3 addresses", "Only can register 3 addresses", "Address.LimitReached", ErrorType.Conflict);

}