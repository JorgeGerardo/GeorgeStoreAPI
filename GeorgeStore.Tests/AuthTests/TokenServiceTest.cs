using GeorgeStore.Features.Auth;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;

namespace GeorgeStore.Tests.AuthTests;

public class TokenServiceTest
{
    [Fact]
    public void GenerateToken_ShouldContainUserIdClaim()
    {
        // Arrange
        var config = new JWTConfig
        {
            Key = "super-secret-key-super-secret-key123123",
            Issuer = "GeorgeStore",
            Audience = "GeorgeStoreUsers",
            DurationMinutes = 10,
            RefreshTokenDurationMinutes = 60
        };

        var options = new Mock<IOptionsSnapshot<JWTConfig>>();
        options.Setup(o => o.Value).Returns(config);

        var service = new TokenService(options.Object);

        var userId = Guid.NewGuid();

        // Act
        var result = service.GenerateToken(userId);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Token);

        Assert.Contains(jwt.Claims,
            c => c.Type == "UserId" && c.Value == userId.ToString());

        Assert.Equal(config.Issuer, jwt.Issuer);

        Assert.Contains(jwt.Audiences,
            a => a == config.Audience);

        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
    }
}
