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
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
}

public class TokenService(IOptionsSnapshot<JWTConfig> jwt)
{
    private static int durationDays = 100;
    public LoginResponse GenerateToken(Guid UserId)
    {
        List<Claim> claims = GetDefaultClaims(UserId);

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.Key));
        SigningCredentials signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        JwtSecurityToken Token = new JwtSecurityToken(
                jwt.Value.Issuer,
                jwt.Value.Audience,
                claims,
                expires: DateTime.UtcNow.AddDays(durationDays),
                signingCredentials: signIn
        );

        var refreshToken = GenerateRefreshToken();
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(Token), refreshToken);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }

    private List<Claim> GetDefaultClaims(Guid userId)
    {
        return new List<Claim>()
            {
                new Claim("UserId", userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(DateTime.UtcNow).ToString()),
            };
    }

}
