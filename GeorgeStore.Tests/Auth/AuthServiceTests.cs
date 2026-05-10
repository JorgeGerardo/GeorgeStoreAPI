using GeorgeStore.Common.Core;
using GeorgeStore.Features.Auth;
using GeorgeStore.Tests.Common;
using GeorgeStore.Common.Shared;
using Microsoft.Extensions.Options;
using Moq;

namespace GeorgeStore.Tests.Auth;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginTest()
    {
        using var context = ContextHelper.Create();
        Guid userId = new();
        var optionsJwtConfig =CreateJwtConfigOptions();
        AuthService authSrv = new(context, new TokenService(optionsJwtConfig));
        
        //Act
        LoginResponse result = await authSrv.GenerateTokensAsync(userId);
        RefreshToken? refreshToken = context.RefreshTokens.FirstOrDefault();
        string hashRefreshToken = result.RefreshToken.GetHash().GetHashString();
        

        Assert.NotNull(refreshToken);
        Assert.Equal(hashRefreshToken, refreshToken.Token);
    }

    [Fact]
    public async Task RefreshTest()
    {
        using var context = ContextHelper.Create();
        var optionsJwtConfig =CreateJwtConfigOptions();
        AuthService authSrv = new(context, new TokenService(optionsJwtConfig));
        string refreshTokenValue = Guid.NewGuid().ToString();
        string refreshTokenHash = refreshTokenValue.GetHash().GetHashString();
        Guid userId = Guid.NewGuid();

        RefreshToken refreshToken = new()
        {
            CreateAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(10),
            IsRevoked = false,
            Token = refreshTokenHash,
            UserId = userId,
        };
        context.RefreshTokens.Add(refreshToken);
        context.SaveChanges();

        //Act
        var result = await authSrv.RefreshTokensAsync(refreshTokenValue);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Refresh_Revoked_Test()
    {
        using var context = ContextHelper.Create();
        var optionsJwtConfig =CreateJwtConfigOptions();
        AuthService authSrv = new(context, new TokenService(optionsJwtConfig));
        string refreshTokenValue = Guid.NewGuid().ToString();
        string refreshTokenHash = refreshTokenValue.GetHash().GetHashString();
        Guid userId = Guid.NewGuid();

        RefreshToken refreshToken = new()
        {
            CreateAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(10),
            IsRevoked = true, //Focus
            Token = refreshTokenHash,
            UserId = userId,
        };
        context.RefreshTokens.Add(refreshToken);
        context.SaveChanges();

        //Act
        var result = await authSrv.RefreshTokensAsync(refreshTokenValue);

        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.Revoked, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Fact]
    public async Task Refresh_Expired_Test()
    {
        using var context = ContextHelper.Create();
        var optionsJwtConfig =CreateJwtConfigOptions();
        AuthService authSrv = new(context, new TokenService(optionsJwtConfig));
        string refreshTokenValue = Guid.NewGuid().ToString();
        string refreshTokenHash = refreshTokenValue.GetHash().GetHashString();
        Guid userId = Guid.NewGuid();

        RefreshToken refreshToken = new()
        {
            CreateAt = DateTime.UtcNow.AddMinutes(-30),
            Expires = DateTime.UtcNow.AddMinutes(-10),
            IsRevoked = false,
            Token = refreshTokenHash,
            UserId = userId,
        };
        context.RefreshTokens.Add(refreshToken);
        context.SaveChanges();

        //Act
        var result = await authSrv.RefreshTokensAsync(refreshTokenValue);

        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.Expired, result.Error);

    }


    [Fact]
    public async Task Refresh_NotFound_Test()
    {
        using var context = ContextHelper.Create();
        var optionsJwtConfig =CreateJwtConfigOptions();
        AuthService authSrv = new(context, new TokenService(optionsJwtConfig));
        string randomString = Guid.NewGuid().ToString();

        //Act
        var result = await authSrv.RefreshTokensAsync(randomString);

        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.Notfound, result.Error);
    }


    [Fact]
    public async Task LogoutTest()
    {
        using var context = ContextHelper.Create();
        var optionsJwtConfig =CreateJwtConfigOptions();
        AuthService authSrv = new(context, new TokenService(optionsJwtConfig));
        string refreshTokenValue = Guid.NewGuid().ToString();
        string refreshTokenHash = refreshTokenValue.GetHash().GetHashString();
        Guid userId = Guid.NewGuid();

        RefreshToken refreshToken = new()
        {
            CreateAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(10),
            IsRevoked = false,
            Token = refreshTokenHash,
            UserId = userId,
        };
        context.RefreshTokens.Add(refreshToken);
        context.SaveChanges();


        //Act
        Result result = await authSrv.LogoutAsync(refreshTokenValue);

        Assert.True(result.IsSuccess);
        Assert.True(refreshToken.IsRevoked);

    }


    //Common arranges
    private static IOptionsSnapshot<JWTConfig> CreateJwtConfigOptions()
    {
        var jwtConfig = CreateJwtConfig();
        var jwtOptionsMock = new Mock<IOptionsSnapshot<JWTConfig>>();
        jwtOptionsMock.Setup(o => o.Value).Returns(jwtConfig);
        return jwtOptionsMock.Object;
    }

    private static JWTConfig CreateJwtConfig(int DurationMinutes = 10, int RefreshTokenDurationMinutes = 10)
    {
        return new()
        {
            Key = "Secret-key-Secret-key-Secret-key-Secret-key-Secret-key12323ls",
            Issuer = "George Corporation",
            Audience = "GeorgeStore",
            DurationMinutes = DurationMinutes,
            RefreshTokenDurationMinutes = RefreshTokenDurationMinutes,
        };
    }

}
