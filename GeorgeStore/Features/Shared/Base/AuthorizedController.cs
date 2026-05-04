using GeorgeStore.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace GeorgeStore.Features.Shared.Base;

[Authorize]
public abstract class AuthorizedController : ApiControllerBase
{
    protected Guid UserId => User.GetUserId();
}
