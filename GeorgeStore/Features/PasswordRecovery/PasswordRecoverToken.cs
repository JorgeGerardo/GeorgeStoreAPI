using GeorgeStore.Common.Core;

namespace GeorgeStore.Features.PasswordRecovery;

public class PasswordRecoverToken : Entity
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public static PasswordRecoverToken Create(Guid UserId, string TokenHash, string? IpAddress, string? UserAgent)
    {
        return new()
        {
            UserId = UserId,
            TokenHash = TokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            IpAddress = IpAddress,
            UserAgent = UserAgent,
        };
    }
}