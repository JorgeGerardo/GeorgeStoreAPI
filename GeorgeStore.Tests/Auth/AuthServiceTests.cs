using GeorgeStore.Common.Core;
using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Auth;
using GeorgeStore.Tests.Common;
using GeorgeStore.Tests.Factories;

namespace GeorgeStore.Tests.Auth;

public class AuthServiceTests
{
    [Fact]
    public async Task GenerateTokensTest()
    {
        using var context = ContextHelper.Create();
        AuthService authSrv = AuthFactory.CreateService(context);
        Guid userId = new();
        
        //Act
        LoginResponse result = await authSrv.GenerateTokensAsync(userId);
        RefreshToken? refreshToken = context.RefreshTokens.FirstOrDefault();
        string hashRefreshToken = result.RefreshToken.GetHash().GetHashString();
        //Assert        
        Assert.NotNull(refreshToken);
        Assert.Equal(hashRefreshToken, refreshToken.Token);
    }

    [Fact]
    public async Task RefreshTest()
    {
        using var context = ContextHelper.Create();
        AuthService authSrv = AuthFactory.CreateService(context);
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
        //Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Refresh_Revoked_Test()
    {
        using var context = ContextHelper.Create();
        AuthService authSrv = AuthFactory.CreateService(context);
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
        AuthService authSrv = AuthFactory.CreateService(context);
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
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.Expired, result.Error);

    }


    [Fact]
    public async Task Refresh_NotFound_Test()
    {
        using var context = ContextHelper.Create();
        AuthService authSrv = AuthFactory.CreateService(context);
        string randomString = Guid.NewGuid().ToString();

        //Act
        var result = await authSrv.RefreshTokensAsync(randomString);
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RefreshTokenErrors.Notfound, result.Error);
    }


    [Fact]
    public async Task LogoutTest()
    {
        using var context = ContextHelper.Create();
        AuthService authSrv = AuthFactory.CreateService(context);
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
        //Assert
        Assert.True(result.IsSuccess);
        Assert.True(refreshToken.IsRevoked);

    }

}
