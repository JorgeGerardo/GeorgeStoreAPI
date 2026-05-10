using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace GeorgeStore.Tests.Users;

public class UserServiceTest
{
    [Fact]
    public async Task Login_Failure_UserNotFound()
    {
        using var context = ContextHelper.Create();
        User user = CreateUser(context);
        var userManager = CreateUserManager(user);
        UserService userSrv = new(userManager);

        //Act
        var result = await userSrv.Login("juanita@gmail.com", "Password123!");
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserError.Notfound, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]//TODO: Pending
    public async Task Login_Failure_InvalidCredentials()
    {
        using var context = ContextHelper.Create();
        User user = CreateUser(context);
        var userManager = CreateUserManager(user);
        UserService userSrv = new(userManager);

        //Act
        var result = await userSrv.Login(user.Email!, "IcorrectPassword123!");
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserError.InvalidCredentials, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public async Task GetProfile_UserNotFound()
    {
        using var context = ContextHelper.Create();
        User user = CreateUser(context);
        var userManager = CreateUserManager(user);
        UserService userSrv = new(userManager);
        //Act
        var result = await userSrv.GetProfile(Guid.NewGuid());
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserError.Notfound, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public async Task GetProfile()
    {
        using var context = ContextHelper.Create();
        User user = CreateUser(context);
        var userManager = CreateUserManager(user);
        UserService userSrv = new(userManager);
        //Act
        var result = await userSrv.GetProfile(user.Id);
        //Assert
        Assert.True(result.IsSuccess);
        UserDataDto profileDto = result.Value;
        Assert.Equal(user.Email!, profileDto.Email);
        Assert.Equal(user.UserName, profileDto.UserName);
    }

    [Fact]
    public async Task ExistTest()
    {
        using var context = ContextHelper.Create();
        User user = CreateUser(context);
        var userManager = CreateUserManager(user);
        UserService userSrv = new(userManager);

        //Act
        var result = await userSrv.Exist(user.Email!);
        //Assert
        Assert.True(result.IsSuccess);
    }

    //Common arranges
    private static UserManager<User> CreateUserManager(User user)
    {
        var store = new Mock<IUserStore<User>>();
        PasswordHasher<User> passwordHasher = new();
        var userManager = new Mock<UserManager<User>>(store.Object, null!, passwordHasher, null!, null!, null!, null!, null!, null!);
        userManager.Setup(u => u.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManager.Setup(u => u.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        return userManager.Object;
    }

    private static User CreateUser(GeorgeStoreContext context)
    {
        User user = new("Jorguito", "jorguito@gmail.com");
        context.Add(user);
        context.SaveChanges();
        return user;
    }

}
