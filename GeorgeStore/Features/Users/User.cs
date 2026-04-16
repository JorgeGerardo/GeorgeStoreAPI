using GeorgeStore.Features.Carts;
using Microsoft.AspNetCore.Identity;

namespace GeorgeStore.Features.Users;

public class User : IdentityUser<Guid>
{
    public override Guid Id { get; set; }
    public List<Cart> Carts { get; set; } = [];
}
