using GeorgeStore.Common.Core;
using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Auth;
using GeorgeStore.Features.PasswordRecovery;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Data;
using GeorgeStore.Infrastructure.Email.Brevo;
using GeorgeStore.Tests.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace GeorgeStore.Tests.PasswordRecoveries;

public class RecoverPasswordServiceTests
{
    [Fact]
    public async Task SendRecoverEmailTest()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        RecoverPasswordService recoverPasswordSvc = CreateRecoverPasswordService(context, user);
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

        User user = new User("Jorguito88", email) 
            { PasswordHash = oldPassword.GetHash().GetHashString() };
        context.Users.Add(user);
        context.SaveChanges();


        RecoverPasswordService recoverPasswordSvc = CreateRecoverPasswordService(context, user);



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
        RecoverPasswordService recoverPasswordSvc = CreateRecoverPasswordService(context, user);

        //Act
        Result result = await recoverPasswordSvc.RecoverAsync("FakeRefreshToken", "new password123");
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordRecoverTokenError.NotFound, result.Error);
    }

    [Fact]
    public async Task Recover_Failure_TokenExpired()
    {
        using var context = ContextHelper.Create();
        User user = ContextHelper.CreateUser(context);
        RecoverPasswordService recoverPasswordSvc = CreateRecoverPasswordService(context, user);

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

        RecoverPasswordService recoverPasswordSvc = CreateRecoverPasswordService(context, user);
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
        User user = new User("Jorguito88", "jorguito@gmail.com");
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

        RecoverPasswordService recoverPasswordSvc = CreateRecoverPasswordService(context, user);

        //Act
        Result result = await recoverPasswordSvc.RecoverAsync(recoverToken, "NewPassword123");
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordRecoverTokenError.UserNotFound, result.Error);
    }

    //Common arranges
    private static IEmailSender CreateEmailSender()
    {
        var emailSender = new Mock<IEmailSender>();
        emailSender.Setup(e => e.Send("", "", "", "", "", "")).Returns(Task.CompletedTask);
        return emailSender.Object;
    }
    private static IOptionsSnapshot<BrevoOptions> CreateBrevoOptionsMock()
    {
        BrevoOptions opts = new()
        {
            ApiKey = "",
            EmailSender = "",
            ResetPasswordUrl = ""
        };

        var brevoConfig = opts;
        var brevoOptionsMock = new Mock<IOptionsSnapshot<BrevoOptions>>();
        brevoOptionsMock.Setup(o => o.Value).Returns(brevoConfig);
        return brevoOptionsMock.Object;
    }

    private static IOptionsSnapshot<JWTConfig> CreateJwtConfigOptions(JWTConfig config)
    {
        var jwtConfig = CreateJwtConfig();
        var jwtOptionsMock = new Mock<IOptionsSnapshot<JWTConfig>>();
        jwtOptionsMock.Setup(o => o.Value).Returns(jwtConfig);
        return jwtOptionsMock.Object;
    }

    private static UserManager<User> CreateUserManager(Guid userId, string email, User user)
    {
        var store = new Mock<IUserStore<User>>();
        var passwordHasher = new PasswordHasher<User>();
        var userManager = new Mock<UserManager<User>>(store.Object, null!, passwordHasher, null!, null!, null!, null!, null!, null!);
        userManager.Setup(u => u.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
        userManager.Setup(u => u.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        return userManager.Object;
    }

    private static JWTConfig CreateJwtConfig(int DurationMinutes = 10, int RefreshTokenDurationMinutes = 10)
    {
        return new()
        {
            Key = "",
            Issuer = "",
            Audience = "GeorgeStore",
            DurationMinutes = DurationMinutes,
            RefreshTokenDurationMinutes = RefreshTokenDurationMinutes,
        };
    }

    private RecoverPasswordService CreateRecoverPasswordService(GeorgeStoreContext context, User user)
    {
        UserManager<User> userManager = CreateUserManager(user.Id, user.Email!, user);
        IOptionsSnapshot<BrevoOptions> brevoOptionsMock = CreateBrevoOptionsMock();
        IOptionsSnapshot<JWTConfig> iSnapshotJwt = CreateJwtConfigOptions(CreateJwtConfig(10, 10));
        IEmailSender emailSender = CreateEmailSender();
        return new(userManager, context, emailSender, brevoOptionsMock, iSnapshotJwt);

    }

}
