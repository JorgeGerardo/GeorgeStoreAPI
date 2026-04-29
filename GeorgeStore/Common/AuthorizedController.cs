using GeorgeStore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Common;

[Authorize]
public abstract class AuthorizedController : ControllerBase
{
    protected Guid UserId => User.GetUserId();
}