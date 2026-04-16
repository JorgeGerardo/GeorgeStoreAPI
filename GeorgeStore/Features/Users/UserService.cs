using GeorgeStore.Common;
using Microsoft.AspNetCore.Identity;

namespace GeorgeStore.Features.Users;

public class UserService(UserManager<User> manager) : IUserService
{
    public async Task<Result<User>> Exist(string email)
    {
        User? user = await manager.FindByEmailAsync(email);
        return user is null ?
            Result.Failure<User>(UserError.Notfound) : 
            Result.Success(user);
    }

    public async Task<Result<User>> Login(string email, string password)
    {
        var result = await Exist(email);
        if (!result.IsSuccess)
            return Result.Failure<User>(UserError.Notfound);

        return await manager.CheckPasswordAsync(result.Value, password) is false ?
            Result.Failure<User>(UserError.InvalidCredentials) :
            Result.Success(result.Value);
    }

    public async Task<Result> Register(string userName, string email, string password)
    {
        User newUser = new()
        {
            UserName = userName,
            Email = email
        };
        var result = await manager.CreateAsync(newUser, password);
        
        if (!result.Succeeded)
            return Result.Failure<bool>(new Error("Register error", result.Errors.First().Description, result.Errors.First().Code));

        return Result.Success();
    }
}
