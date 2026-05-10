using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Features.Auth;
using GeorgeStore.Features.PasswordRecovery;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Infrastructure.Email.Brevo;
using GeorgeStore.Tests.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GeorgeStore.Tests.Factories;

internal static class RecoverPasswordFactory
{
    public static RecoverPasswordService CreateService(GeorgeStoreContext context, User user)
    {
        UserManager<User> userManager = ConfigurationHelper.CreateUserManager(user);
        IOptionsSnapshot<BrevoOptions> brevoOptionsMock = ConfigurationHelper.CreateBrevoOptionsMock();
        IOptionsSnapshot<JWTConfig> iSnapshotJwt = ConfigurationHelper.CreateJwtConfigOptions(ConfigurationHelper.CreateJwtConfig(10, 10));
        IEmailSender emailSender = ConfigurationHelper.CreateEmailSender();
        return new(userManager, context, emailSender, brevoOptionsMock, iSnapshotJwt);

    }

}
