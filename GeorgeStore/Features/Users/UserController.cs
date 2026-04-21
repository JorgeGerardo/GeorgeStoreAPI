using GeorgeStore.Common;
using GeorgeStore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Users;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService, TokenService tokenService) : ControllerBase
{
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDataDto>> GetProfile()
    {
        Guid userId = HttpContext.User.GetUserId();
        var result = await userService.GetProfile(userId);
        return result.IsSuccess ? Ok(result.Value) : NotFound(ProblemDetailFactory.FromError(result.Error));

    }

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

