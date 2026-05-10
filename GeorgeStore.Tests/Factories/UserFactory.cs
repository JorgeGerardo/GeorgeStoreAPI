using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace GeorgeStore.Tests.Factories;

internal static class UserFactory
{
    public static UserManager<User> CreateUserManager(User user)
    {
        var store = new Mock<IUserStore<User>>();
        PasswordHasher<User> passwordHasher = new();
        var userManager = new Mock<UserManager<User>>(store.Object, null!, passwordHasher, null!, null!, null!, null!, null!, null!);
        userManager.Setup(u => u.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManager.Setup(u => u.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        return userManager.Object;
    }

    public static User CreateUser(GeorgeStoreContext context)
    {
        User user = new("Jorguito", "jorguito@gmail.com");
        context.Add(user);
        context.SaveChanges();
        return user;
    }

}
