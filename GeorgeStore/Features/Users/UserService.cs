using GeorgeStore.Common.Shared;
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

    public async Task<Result<UserDataDto>> GetProfile(Guid userId)
    {
        User? user = await manager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure<UserDataDto>(UserError.Notfound);

        return Result.Success(new UserDataDto(user.Email!, user.UserName!, user.DateRegister));
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
        User newUser = new(userName, email);
        var result = await manager.CreateAsync(newUser, password);
        
        if (result.Succeeded)
            return Result.Success();


        var errors = result.Errors
            .Select(e => $"{e.Description}")
            .ToArray();

        return Result.Failure(new Error(
            "Register error",
            string.Join(", ", errors),
            "User.InvalidData",
            ErrorType.Validation
        ));
    }
}
