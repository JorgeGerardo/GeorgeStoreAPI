using System.Security.Claims;

namespace GeorgeStore.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        string? userId = user.FindFirst("UserId")?.Value;

        if (userId is not null)
            return Guid.Parse(userId);

        throw new UnauthorizedAccessException();
    }
}
