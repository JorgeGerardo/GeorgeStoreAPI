using GeorgeStore.Common.Core;

namespace GeorgeStore.Features.Auth;

public sealed class RefreshToken : Entity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;

    public static RefreshToken Create(Guid UserId, string RefreshToken)
    {
        return new RefreshToken
        {
            CreateAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(120),
            IsRevoked = false,
            Token = RefreshToken,
            UserId = UserId,
        };

    }

}

public sealed record RefreshRequest(string RefreshToken);
