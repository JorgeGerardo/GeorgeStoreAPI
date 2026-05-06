using GeorgeStore.Common.Core;
using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Auth;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Infrastructure.Email;
using GeorgeStore.Infrastructure.Email.Brevo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GeorgeStore.Features.PasswordRecovery;

public class RecoverPasswordService(UserManager<User> manager, GeorgeStoreContext context, IEmailSender emailSender, IOptionsSnapshot<BrevoOptions> opts, IOptionsSnapshot<JWTConfig> jwt)
{
    public async Task<Result> SendRecoverEmail(RecoverPassowrdDto request, string? IpAddress, string? Agent)
    {
        User? user = await manager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Success();

        var token = Guid.NewGuid().ToString();
        var tokenHasthString = token.GetHash().GetHashString();
        PasswordRecoverToken newResetToken = PasswordRecoverToken.Create(user.Id, tokenHasthString, IpAddress, Agent, jwt.Value.RefreshTokenDurationMinutes);
        context.PasswordResetTokens.Add(newResetToken);

        await emailSender.Send(
            "Recover password",
            opts.Value.EmailSender,
            "George Store",
            user.Email!,
            user.UserName!,
            EmailTemplateRenderer.Render(
                "Templates/Emails/reset-password.html",
                new Dictionary<string, string>
                {
                    ["name"] = user.UserName ?? "Dear user",
                    ["resetUrl"] = $"{opts.Value.ResetPasswordUrl}?prt={token}"
                }
            )
        );

        await context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> Recover(string PasswordResetToken, string NewPassword)
    {
        var tokenHash = PasswordResetToken.GetHash().GetHashString();
        var token = await context.PasswordResetTokens.FirstOrDefaultAsync(tk => tk.TokenHash == tokenHash);
        if(token is null)
            return Result.Failure(PasswordRecoverTokenError.NotFound);

        if (token.ExpiresAt < DateTime.UtcNow)
            return Result.Failure(PasswordRecoverTokenError.TokenExpired);

        if(token.IsUsed)
            return Result.Failure(PasswordRecoverTokenError.TokenUsed);

        var user = await manager.FindByIdAsync(token.UserId.ToString());

        if (user is null)
            return Result.Failure(PasswordRecoverTokenError.TokenUsed);


        user.PasswordHash = manager.PasswordHasher.HashPassword(user, NewPassword);
        await manager.UpdateAsync(user);
        await context.SaveChangesAsync();
        return Result.Success();
    }
}
