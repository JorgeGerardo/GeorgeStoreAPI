using GeorgeStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GeorgeStore.Services;

public record LoginResponse(string Token);
public record JWTConfig
{
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
}

public class TokenService(IOptionsSnapshot<JWTConfig> jwt)
{
    private static int durationDays = 100;
    public LoginResponse GenerateToken(User user)
    {
        List<Claim> claims = GetDefaultClaims(user.Id);

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.Key));
        SigningCredentials signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        JwtSecurityToken Token = new JwtSecurityToken(
                jwt.Value.Issuer,
                jwt.Value.Audience,
                claims,
                expires: DateTime.UtcNow.AddDays(durationDays),
                signingCredentials: signIn
        );
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(Token));
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

