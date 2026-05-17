using GeorgeStore.Common.Shared;

namespace GeorgeStore.Features.PasswordRecovery;

public static class PasswordRecoverTokenError
{
    public static readonly Error TokenExpired =
        new("Token has expired", "This token has expired, try again", "PasswordResetError.TokenExpired", ErrorType.Validation);

    public static readonly Error TokenUsed =
        new("Token used", "Token has already been used", "PasswordResetError.TokenUsed", ErrorType.Validation);

    public static readonly Error NotFound =
        new("Token was not found", "This token isn't exist", "PasswordResetError.NotFound", ErrorType.NotFound);

    public static readonly Error UserNotFound =
        new("User not found", "Can't find this user", "PasswordResetError.UserNotFound", ErrorType.NotFound);

    public static readonly Error AttempsLimitReached =
        new("Too many attempts", "You can only request 3 password reset emails per hour. Please try again later.", "PasswordResetError.AttemptsLimitReached", ErrorType.Validation);

}