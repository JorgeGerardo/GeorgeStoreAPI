using GeorgeStore.Common;

namespace GeorgeStore.Features.Users;

public static class UserError
{
    public static readonly Error Notfound = 
        new("User not found", "Can't find user", "User.Notfound", ErrorType.NotFound);

    public static readonly Error InvalidCredentials = 
        new("Invalid credentials", "Try with another credentials", "User.InvalidCredentials", ErrorType.Validation);
    
}