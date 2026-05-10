using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Features.Auth;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Email.Brevo;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace GeorgeStore.Tests.Common;

internal static class ConfigurationHelper
{
    public static IEmailSender CreateEmailSender()
    {
        var emailSender = new Mock<IEmailSender>();
        emailSender.Setup(e => e.Send("", "", "", "", "", "")).Returns(Task.CompletedTask);
        return emailSender.Object;
    }

    public static IOptionsSnapshot<BrevoOptions> CreateBrevoOptionsMock()
    {
        BrevoOptions opts = new()
        {
            ApiKey = "API-KEY-086ee08f663c3c347311b48b0a1c7d8fecb5d9821dcbf1487e10ce02a358",
            EmailSender = "notsofar@gmail.com",
            ResetPasswordUrl = "http://localhost:4200/auth/recover"
        };

        var brevoConfig = opts;
        var brevoOptionsMock = new Mock<IOptionsSnapshot<BrevoOptions>>();
        brevoOptionsMock.Setup(o => o.Value).Returns(brevoConfig);
        return brevoOptionsMock.Object;
    }

    public static IOptionsSnapshot<JWTConfig> CreateJwtConfigOptions(JWTConfig config)
    {
        var jwtConfig = CreateJwtConfig();
        var jwtOptionsMock = new Mock<IOptionsSnapshot<JWTConfig>>();
        jwtOptionsMock.Setup(o => o.Value).Returns(jwtConfig);
        return jwtOptionsMock.Object;
    }

    public static JWTConfig CreateJwtConfig(int DurationMinutes = 10, int RefreshTokenDurationMinutes = 10)
    {
        return new()
        {
            Key = "086ee08f663c3c347asdfjbkeytesta1c7d8fecb5d9821dcbf1487e10ce02a35832se",
            Issuer = "IssuerTest",
            Audience = "GeorgeStore",
            DurationMinutes = DurationMinutes,
            RefreshTokenDurationMinutes = RefreshTokenDurationMinutes,
        };
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
