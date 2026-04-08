using GeorgeStore.Core;
using Microsoft.AspNetCore.Identity;

namespace GeorgeStore.Models;

public class User : IdentityUser<Guid>
{
    public int SapitoFeliz { get; set; }
    public override Guid Id { get; set; }
}


public static class UserError
{
    public static readonly Error Notfound = 
        new("User not found", "Can't find user", "User.Notfound");

    public static readonly Error InvalidCredentials = 
        new("Invalid credentials", "Try with another credentials", "User.InvalidCredentials");

}