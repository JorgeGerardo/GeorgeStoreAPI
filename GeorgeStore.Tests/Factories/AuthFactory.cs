using GeorgeStore.Features.Auth;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Tests.Common;
using Microsoft.Extensions.Options;

namespace GeorgeStore.Tests.Factories;

internal static class AuthFactory
{
    public static AuthService CreateService(GeorgeStoreContext context)
    {
        IOptionsSnapshot<JWTConfig> optionsJwtConfig = ConfigurationHelper.CreateJwtConfigOptions();
        return new(context, new TokenService(optionsJwtConfig));
    }

}
