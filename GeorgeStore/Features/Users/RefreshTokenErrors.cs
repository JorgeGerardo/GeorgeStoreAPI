using GeorgeStore.Common;

namespace GeorgeStore.Features.Users;

public static class RefreshTokenErrors
{
    public static readonly Error Notfound =
        new("Token not found", "This token isn't exist", "RefreshToken.Notfound", ErrorType.NotFound);

    public static readonly Error Expired =
        new("Token expired", "Refresh token has expired", "RefreshToken.Expired", ErrorType.Validation);
    
    public static readonly Error Revoked =
        new("Token revoked", "Refresh token has be revoked", "RefreshToken.Revoked", ErrorType.Validation);

}