using GeorgeStore.Features.Users;
using GeorgeStore.Tests.Common;

namespace GeorgeStore.Tests.Users;

public class UserServiceTests
{
    [Fact]
    public async Task Login_Failure_UserNotFound()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        var userManager = ConfigurationHelper.CreateUserManager(user);
        UserService userSrv = new(userManager);

        //Act
        var result = await userSrv.Login("juanita@gmail.com", "Password123!");
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserError.Notfound, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public async Task Login_Failure_InvalidCredentials()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        var userManager = ConfigurationHelper.CreateUserManager(user);
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
        User user = ContextHelper.CreateUser(context);
        var userManager = ConfigurationHelper.CreateUserManager(user);
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
        User user = ContextHelper.CreateUser(context);
        var userManager = ConfigurationHelper.CreateUserManager(user);
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
        User user = ContextHelper.CreateUser(context);
        var userManager = ConfigurationHelper.CreateUserManager(user);
        UserService userSrv = new(userManager);

        //Act
        var result = await userSrv.Exist(user.Email!);
        //Assert
        Assert.True(result.IsSuccess);
    }

}
