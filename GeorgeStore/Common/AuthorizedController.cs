using GeorgeStore.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace GeorgeStore.Common;

[Authorize]
public abstract class AuthorizedController : ApiControllerBase
{
    protected Guid UserId => User.GetUserId();
}
