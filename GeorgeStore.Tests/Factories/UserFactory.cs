using GeorgeStore.Features.Users;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace GeorgeStore.Tests.Factories;

internal static class UserFactory
{
    public static UserService CreateService(User user)
    {
        var userManager = CreateUserManager(user);
        return new(userManager);
    }

    public static UserManager<User> CreateUserManager(User user)
    {
        var store = new Mock<IUserStore<User>>();
        var passwordHasher = new PasswordHasher<User>();
        var userManager = new Mock<UserManager<User>>(store.Object, null!, passwordHasher, null!, null!, null!, null!, null!, null!);
        userManager.Setup(u => u.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManager.Setup(u => u.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        return userManager.Object;
    }

}
