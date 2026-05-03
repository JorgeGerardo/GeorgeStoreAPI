using GeorgeStore.Common;
using GeorgeStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace GeorgeStore.Features.Users;

public class AuthService(GeorgeStoreContext context, TokenService tokenService)
{
    public async Task<LoginResponse> Login(Guid UserId)
    {
        var tokens = tokenService.GenerateToken(UserId);

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(tokens.RefreshToken));
        string hashString = Convert.ToBase64String(hash);

        RefreshToken newRefreshToken = RefreshToken.Create(UserId, hashString);

        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync();
        return tokens;
    }

    public async Task<Result<LoginResponse>> Refresh(string refreshToken)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        string hashString = Convert.ToBase64String(hash);

        var token = await context.RefreshTokens.FirstOrDefaultAsync(rf => rf.Token == hashString);
        if (token is null)
            return Result.Failure<LoginResponse>(RefreshTokenErrors.Notfound);

        if (token.IsRevoked)
            return Result.Failure<LoginResponse>(RefreshTokenErrors.Revoked);

        if (token.Expires < DateTime.UtcNow)
            return Result.Failure<LoginResponse>(RefreshTokenErrors.Expired);

        token.IsRevoked = true;
        LoginResponse newTokens = tokenService.GenerateToken(token.UserId);
        byte[] newHash = SHA256.HashData(Encoding.UTF8.GetBytes(newTokens.RefreshToken));
        string newHashString = Convert.ToBase64String(newHash);


        RefreshToken newRefreshToken = RefreshToken.Create(token.UserId, newHashString);
        context.RefreshTokens.Add(newRefreshToken);

        await context.SaveChangesAsync();
        return Result.Success(newTokens);
    }

    public async Task<Result> Logout(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Success();

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        string hashString = Convert.ToBase64String(hash);
        var token = await context.RefreshTokens.FirstOrDefaultAsync(rf => rf.Token == hashString && !rf.IsRevoked);

        if (token is null)
            return Result.Success();

        token.IsRevoked = true;
        await context.SaveChangesAsync();
        return Result.Success();
    }


}
