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
        var userManager = CreateUserManager(user.Id, "jorguito@gmail.com", user);
        UserService userSrv = new(userManager);

        //Act
        var result = await userSrv.Login("juanita@gmail.com", "Password123!");


        Assert.False(result.IsSuccess);
        Assert.Equal(UserError.Notfound, result.Error);
    }

    [Fact]
    public async Task GetProfile_UserNotFound()
    {
        using var context = ContextHelper.Create();
        const string email = "jorguito@gmail.com";
        User user = CreateUser(context);
        var userManager = CreateUserManager(user.Id, email, user);
        UserService userSrv = new(userManager);

        var result = await userSrv.GetProfile(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal(UserError.Notfound, result.Error);
    }

    [Fact]
    public async Task GetProfile()
    {
        using var context = ContextHelper.Create();
        const string email = "jorguito@gmail.com";
        User user = CreateUser(context);
        var userManager = CreateUserManager(user.Id, email, user);
        UserService userSrv = new(userManager);

        var result = await userSrv.GetProfile(user.Id);

        Assert.True(result.IsSuccess);
        var profileDto = result.Value;
        Assert.NotNull(profileDto);
        Assert.Equal(email, profileDto.Email);
    }

    [Fact]
    public async Task ExistTest()
    {
        using var context = ContextHelper.Create();
        const string email = "jorguito@gmail.com";
        User user = CreateUser(context);
        var userManager = CreateUserManager(user.Id, email, user);
        UserService userSrv = new(userManager);


        var result = await userSrv.Exist(email);
        Assert.True(result.IsSuccess);
    }

    //Common arranges
    private static UserManager<User> CreateUserManager(Guid userId, string email, User user)
    {
        var store = new Mock<IUserStore<User>>();
        var passwordHasher = new PasswordHasher<User>();
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
