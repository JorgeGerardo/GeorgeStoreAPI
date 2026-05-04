using GeorgeStore.Features.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Users;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService) : AuthorizedController
{
    [HttpGet("profile")]
    public async Task<ActionResult<UserDataDto>> GetProfile()
    {
        var result = await userService.GetProfile(UserId);
        return HandleResult(result);

    }

}
