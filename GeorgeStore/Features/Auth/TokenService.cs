using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GeorgeStore.Features.Auth;

public record LoginResponse(string Token, string RefreshToken);
public record JWTConfig
{
    public required string Key { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required int DurationMinutes { get; init; }
    public required int RefreshTokenDurationMinutes { get; init; }
}

public class TokenService(IOptionsSnapshot<JWTConfig> jwt)
{
    public LoginResponse GenerateToken(Guid UserId)
    {
        List<Claim> claims = GetDefaultClaims(UserId);

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwt.Value.Key));
        SigningCredentials signIn = new(key, SecurityAlgorithms.HmacSha256Signature);

        JwtSecurityToken Token = new(
                jwt.Value.Issuer,
                jwt.Value.Audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(jwt.Value.DurationMinutes),
                signingCredentials: signIn
        );

        var refreshToken = GenerateRefreshToken();
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(Token), refreshToken);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    private static List<Claim> GetDefaultClaims(Guid userId)
    {
        return
            [
                new("UserId", userId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(DateTime.UtcNow).ToString()),
            ];
    }

}
