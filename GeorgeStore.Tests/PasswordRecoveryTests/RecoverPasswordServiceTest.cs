using GeorgeStore.Common.Core;
using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Features.Auth;
using GeorgeStore.Features.PasswordRecovery;
using GeorgeStore.Features.Users;
using GeorgeStore.Infrastructure.Email.Brevo;
using GeorgeStore.Tests.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace GeorgeStore.Tests.PasswordRecoveryTests;

public class RecoverPasswordServiceTest
{
    [Fact]
    public async Task SendRecoverEmailTest()
    {
        Guid userId = Guid.NewGuid();
        const string email = "jorguito@gmail.com";
        var context = ContextHelper.CreateContext();
        var emailSender = CreateEmailSender();

        var userManager = CreateUserManager(userId, email, null);
        var brevoOptionsMock = CreateBrevoOptionsMock();
        var iSnapshotJwt = CreateJwtConfigOptions(CreateJwtConfig(10, 10));

        var recoverPasswordSvc = new RecoverPasswordService(userManager, context, emailSender, brevoOptionsMock, iSnapshotJwt);
        var request = new RecoverPassowrdDto(email);

        //Act
        await recoverPasswordSvc.SendRecoverEmailAsync(request, null, null);
        var tokenEntity = context.PasswordResetTokens.FirstOrDefault();

        Assert.NotNull(tokenEntity);
        Assert.Equal(userId, tokenEntity.UserId);
    }

    [Fact]
    public async Task RecoverTest()
    {
        var context = ContextHelper.CreateContext();

        Guid userId = Guid.NewGuid();
        const string newPassword = "NewPassword123";
        string newPasswordHash = newPassword.GetHash().GetHashString();
        const string oldPassword = "Password329%923&";
        const string email = "jorguito@gmail.com";

        User user = new User("Jorguito88", email);
        user.PasswordHash = oldPassword.GetHash().GetHashString();
        context.Users.Add(user);
        context.SaveChanges();


        var userManager = CreateUserManager(userId, email, user);
        var brevoOptionsMock = CreateBrevoOptionsMock();
        var iSnapshotJwt = CreateJwtConfigOptions(CreateJwtConfig(10, 10));
        var emailSender = CreateEmailSender();

        var recoverPasswordSvc = new RecoverPasswordService(userManager, context, emailSender, brevoOptionsMock, iSnapshotJwt);



        //Act
        string recoverToken = Guid.NewGuid().ToString();
        PasswordRecoverToken tokenRecoverToken = new()
        {
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            UserId = userId,
            TokenHash = recoverToken.GetHash().GetHashString()
        };
        context.PasswordResetTokens.Add(tokenRecoverToken);
        context.SaveChanges();

        var tokenEntity = context.PasswordResetTokens.FirstOrDefault();

        Assert.NotNull(tokenEntity);
        Assert.Equal(userId, tokenEntity.UserId);
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
        var context = ContextHelper.CreateContext();

        Guid userId = Guid.NewGuid();
        const string email = "jorguito@gmail.com";

        var userManager = CreateUserManager(userId, email, null);
        var brevoOptionsMock = CreateBrevoOptionsMock();
        var iSnapshotJwt = CreateJwtConfigOptions(CreateJwtConfig(10, 10));
        var emailSender = CreateEmailSender();

        var recoverPasswordSvc = new RecoverPasswordService(userManager, context, emailSender, brevoOptionsMock, iSnapshotJwt);



        //Act
        var result = await recoverPasswordSvc.RecoverAsync("FakeRefreshToken", "new password123");
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordRecoverTokenError.NotFound, result.Error);
    }

    [Fact]
    public async Task Recover_Failure_TokenExpired()
    {
        var context = ContextHelper.CreateContext();

        Guid userId = Guid.NewGuid();
        const string email = "jorguito@gmail.com";

        var userManager = CreateUserManager(userId, email, null);
        var brevoOptionsMock = CreateBrevoOptionsMock();
        var iSnapshotJwt = CreateJwtConfigOptions(CreateJwtConfig(10, 10));
        var emailSender = CreateEmailSender();

        var recoverPasswordSvc = new RecoverPasswordService(userManager, context, emailSender, brevoOptionsMock, iSnapshotJwt);
        string recoverToken = Guid.NewGuid().ToString();
        PasswordRecoverToken tokenRecoverToken = new()
        {
            CreatedAt = DateTime.UtcNow.AddMinutes(-20),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-15),
            UserId = userId,
            TokenHash = recoverToken.GetHash().GetHashString()
        };
        context.PasswordResetTokens.Add(tokenRecoverToken);
        context.SaveChanges();



        //Act
        var result = await recoverPasswordSvc.RecoverAsync(recoverToken, "new password123");
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordRecoverTokenError.TokenExpired, result.Error);
    }

    [Fact]
    public async Task Recover_Failure_TokenUsed()
    {
        var context = ContextHelper.CreateContext();

        Guid userId = Guid.NewGuid();
        const string email = "jorguito@gmail.com";

        var userManager = CreateUserManager(userId, email, null);
        var brevoOptionsMock = CreateBrevoOptionsMock();
        var iSnapshotJwt = CreateJwtConfigOptions(CreateJwtConfig(10, 10));
        var emailSender = CreateEmailSender();

        var recoverPasswordSvc = new RecoverPasswordService(userManager, context, emailSender, brevoOptionsMock, iSnapshotJwt);
        string recoverToken = Guid.NewGuid().ToString();
        PasswordRecoverToken tokenRecoverToken = new()
        {
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            UserId = userId,
            TokenHash = recoverToken.GetHash().GetHashString(),
            IsUsed = true
        };
        context.PasswordResetTokens.Add(tokenRecoverToken);
        context.SaveChanges();



        //Act
        var result = await recoverPasswordSvc.RecoverAsync(recoverToken, "new password123");
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordRecoverTokenError.TokenUsed, result.Error);
    }

    [Fact]
    public async Task Recover_Failure_UserNotFound()
    {
        var context = ContextHelper.CreateContext();
        var store = new Mock<IUserStore<User>>();
        var passwordHasher = new PasswordHasher<User>();
        var userManager = new Mock<UserManager<User>>(store.Object, null!, passwordHasher, null!, null!, null!, null!, null!, null!);
        userManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var brevoOptionsMock = CreateBrevoOptionsMock();
        var iSnapshotJwt = CreateJwtConfigOptions(CreateJwtConfig(10, 10));
        var emailSender = CreateEmailSender();


        Guid userId = Guid.NewGuid();
        const string newPassword = "NewPassword123";
        string newPasswordHash = newPassword.GetHash().GetHashString();
        const string oldPassword = "Password329%923&";
        const string email = "jorguito@gmail.com";

        User user = new User("Jorguito88", email);
        user.PasswordHash = oldPassword.GetHash().GetHashString();
        context.Users.Add(user);
        context.SaveChanges();

        var recoverPasswordSvc = new RecoverPasswordService(userManager.Object, context, emailSender, brevoOptionsMock, iSnapshotJwt);

        string recoverToken = Guid.NewGuid().ToString();
        PasswordRecoverToken tokenRecoverToken = new()
        {
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            UserId = userId,
            TokenHash = recoverToken.GetHash().GetHashString()
        };
        context.PasswordResetTokens.Add(tokenRecoverToken);
        context.SaveChanges();

        var tokenEntity = context.PasswordResetTokens.FirstOrDefault();

        Assert.NotNull(tokenEntity);
        Assert.Equal(userId, tokenEntity.UserId);

        context.Users.Remove(user);
        context.SaveChanges();

        //Act
        var result = await recoverPasswordSvc.RecoverAsync(recoverToken, newPassword);
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordRecoverTokenError.UserNotFound, result.Error);
    }


    //Common-arranges
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

    private static UserManager<User> CreateUserManager(Guid userId, string email, User? user)
    {
        var store = new Mock<IUserStore<User>>();
        var passwordHasher = new PasswordHasher<User>();
        var userManager = new Mock<UserManager<User>>(store.Object, null!, passwordHasher, null!, null!, null!, null!, null!, null!);
        user ??= new User("Default user", email) { Id = userId, Email = email };
        userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        userManager.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

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

}
