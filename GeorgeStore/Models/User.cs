using Microsoft.AspNetCore.Identity;

namespace GeorgeStore.Models;

public class User : IdentityUser<Guid>
{
    public int SapitoFeliz { get; set; }
    public override Guid Id { get; set; }
}
