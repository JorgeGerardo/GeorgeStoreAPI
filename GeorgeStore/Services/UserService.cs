using GeorgeStore.Data;
using GeorgeStore.Models;
using Microsoft.AspNetCore.Identity;

namespace GeorgeStore.Services;

public class UserService(UserManager<User> manager) : IUserRepository
{
    public async Task<User?> Exist(string user) => await manager.FindByEmailAsync(user);

    public async Task<User?> Login(string email, string password)
    {
        var user = await Exist(email);
        if (user is null)
            return null;

        return await manager.CheckPasswordAsync(user, password) is false ? null : user;
    }

    public async Task<bool> Register(string userName, string email, string password)
    {
        User newUser = new();
        newUser.UserName = userName;
        newUser.Email = email;
        var result = await manager.CreateAsync(newUser, password);
        if(!result.Succeeded)
            return false;
        return true;
    }
}
