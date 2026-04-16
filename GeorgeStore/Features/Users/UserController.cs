using GeorgeStore.Common;
using GeorgeStore.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Users;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService, TokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest credentials)
    {
        Result<User> result = await userService.Login(credentials.Email, credentials.Password);
        if (!result.IsSuccess)
            return BadRequest(ProblemDetailFactory.FromError(result.Error));

        return Ok(tokenService.GenerateToken(result.Value));
    }

    [HttpPost("register")]
    public async Task<ActionResult> Create(CreateUserRequest credentials)
    {
        Result result = await userService.Register(credentials.UserName, credentials.Email, credentials.Password);
        if (result.IsSuccess)
            return Ok();

        return BadRequest(ProblemDetailFactory.FromError(result.Error));
    }

}

