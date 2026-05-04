using GeorgeStore.Common;

namespace GeorgeStore.Features.PasswordResetTokens;

public static class PasswordResetTokenError
{
    public static readonly Error TokenExpired =
        new("Token has expired", "This token has expired, try again", "PasswordResetError.TokenExpired", ErrorType.Validation);

    public static readonly Error TokenUsed =
        new("Token used", "Token has already been used", "PasswordResetError.TokenUsed", ErrorType.Validation);

    public static readonly Error NotFound =
        new("Token was not found", "This token isn't exist", "PasswordResetError.NotFound", ErrorType.NotFound);

    public static readonly Error UserNotFound =
        new("User not found", "Can't find this user", "PasswordResetError.UserNotFound", ErrorType.NotFound);

}