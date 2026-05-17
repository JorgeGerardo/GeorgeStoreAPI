using Azure.Core;
using GeorgeStore.Common.Core;
using GeorgeStore.Common.Shared;
using GeorgeStore.Features.PasswordRecovery;
using GeorgeStore.Features.Users;
using GeorgeStore.Tests.Common;
using GeorgeStore.Tests.Factories;
using Microsoft.AspNetCore.Identity;

namespace GeorgeStore.Tests.PasswordRecoveries;

public class RecoverPasswordServiceTests
{
    [Fact]
    public async Task SendRecoverEmailTest()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        RecoverPasswordService recoverPasswordSvc = RecoverPasswordFactory.CreateService(context, user);
        RecoverPassowrdDto request = new(user.Email!);

        //Act
        await recoverPasswordSvc.SendRecoverEmailAsync(request, null, null);
        PasswordRecoverToken? tokenEntity = context.PasswordResetTokens.FirstOrDefault(t => t.UserId == user.Id);


        Assert.NotNull(tokenEntity);
        Assert.Equal(user.Id, tokenEntity.UserId);
    }

    [Fact]
    public async Task RecoverTest()
    {
        using var context = ContextHelper.Create();
        const string newPassword = "NewPassword123";
        string newPasswordHash = newPassword.GetHash().GetHashString();
        const string oldPassword = "Password329%923&";
        const string email = "jorguito@gmail.com";

        User user = new("Jorguito88", email) { PasswordHash = oldPassword.GetHash().GetHashString() };
        context.Users.Add(user);
        context.SaveChanges();


        RecoverPasswordService recoverPasswordSvc = RecoverPasswordFactory.CreateService(context, user);



        //Act
        string recoverToken = Guid.NewGuid().ToString();
        PasswordRecoverToken tokenRecoverToken = new()
        {
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            UserId = user.Id,
            TokenHash = recoverToken.GetHash().GetHashString()
        };
        context.PasswordResetTokens.Add(tokenRecoverToken);
        context.SaveChanges();

        var tokenEntity = context.PasswordResetTokens.FirstOrDefault();

        Assert.NotNull(tokenEntity);
        Assert.Equal(user.Id, tokenEntity.UserId);
        var result = await recoverPasswordSvc.RecoverAsync(recoverToken, newPassword);
        Assert.True(result.IsSuccess);

        //Password hasher user use salt in hash creation
        var resultVerify = new PasswordHasher<User>()
            .VerifyHashedPassword(user, user.PasswordHash, newPassword);
        Assert.Equal(PasswordVerificationResult.Success, resultVerify);
    }

    [Fact]
    public async Task Recover_Failure_TokenNotFound()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        RecoverPasswordService recoverPasswordSvc = RecoverPasswordFactory.CreateService(context, user);

        //Act
        Result result = await recoverPasswordSvc.RecoverAsync("FakeRefreshToken", "new password123");
        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordRecoverTokenError.NotFound, result.Error);
    }

    [Fact]
    public async Task Recover_Failure_TokenExpired()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        RecoverPasswordService recoverPasswordSvc = RecoverPasswordFactory.CreateService(context, user);

        string recoverToken = Guid.NewGuid().ToString();
        PasswordRecoverToken tokenRecoverToken = new()
        {
            CreatedAt = DateTime.UtcNow.AddMinutes(-20),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-15),
            UserId = user.Id,
            TokenHash = recoverToken.GetHash().GetHashString()
        };
        context.PasswordResetTokens.Add(tokenRecoverToken);
        context.SaveChanges();

        //Act
        Result result = await recoverPasswordSvc.RecoverAsync(recoverToken, "new password123");

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordRecoverTokenError.TokenExpired, result.Error);
    }

    [Fact]
    public async Task Recover_Failure_TokenUsed()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        RecoverPasswordService recoverPasswordSvc = RecoverPasswordFactory.CreateService(context, user);

        string recoverToken = Guid.NewGuid().ToString();
        PasswordRecoverToken tokenRecoverToken = new()
        {
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            UserId = user.Id,
            TokenHash = recoverToken.GetHash().GetHashString(),
            IsUsed = true
        };
        context.PasswordResetTokens.Add(tokenRecoverToken);
        context.SaveChanges();

        //Act
        Result result = await recoverPasswordSvc.RecoverAsync(recoverToken, "new password123");

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordRecoverTokenError.TokenUsed, result.Error);
    }

    [Fact]
    public async Task Recover_Failure_UserNotFound()
    {
        using var context = ContextHelper.Create();
        User user = new("Jorguito88", "jorguito@gmail.com");
        string recoverToken = Guid.NewGuid().ToString();
        Guid fakeUserId = Guid.NewGuid();
        PasswordRecoverToken tokenRecoverToken = new()
        {
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            UserId = fakeUserId,
            TokenHash = recoverToken.GetHash().GetHashString()
        };
        context.AddRange([tokenRecoverToken, user]);
        context.SaveChanges();

        PasswordRecoverToken? tokenEntity = context.PasswordResetTokens.FirstOrDefault();

        Assert.NotNull(tokenEntity);
        Assert.Equal(fakeUserId, tokenEntity.UserId);

        RecoverPasswordService recoverPasswordSvc = RecoverPasswordFactory.CreateService(context, user);

        //Act
        Result result = await recoverPasswordSvc.RecoverAsync(recoverToken, "NewPassword123");
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordRecoverTokenError.UserNotFound, result.Error);
    }



    [Fact]
    public async Task Recover_Failure_AttempsLimiteReached()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        RecoverPasswordService recoverPasswordSvc = RecoverPasswordFactory.CreateService(context, user);

        string recoverToken = Guid.NewGuid().ToString();
        RecoverPasswordFactory.CreateRandom(context, user, Guid.NewGuid().ToString());
        RecoverPasswordFactory.CreateRandom(context, user, Guid.NewGuid().ToString());
        RecoverPasswordFactory.CreateRandom(context, user, Guid.NewGuid().ToString());

        RecoverPassowrdDto request = new(user.Email!);
        //Act
        Result result =  await recoverPasswordSvc.SendRecoverEmailAsync(request, null, null);

        //Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordRecoverTokenError.AttempsLimitReached, result.Error);
    }

}