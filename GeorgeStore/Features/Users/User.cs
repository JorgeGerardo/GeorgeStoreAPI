using GeorgeStore.Features.Addresses;
using GeorgeStore.Features.Carts;
using Microsoft.AspNetCore.Identity;

namespace GeorgeStore.Features.Users;

public class User : IdentityUser<Guid>
{
    public override Guid Id { get; set; }
    public DateTime DateRegister { get; set; }
    public List<Cart> Carts { get; set; } = [];
    public List<Address> Addresses { get; set; } = [];

    public User(string UserName, string Email)
    {
        this.UserName = UserName;
        this.Email = Email;
        DateRegister = DateTime.UtcNow;
    }
}
