using GeorgeStore.Common;
using GeorgeStore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Users;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService, AuthService authService) : ApiControllerBase
{
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDataDto>> GetProfile()
    {
        Guid userId = HttpContext.User.GetUserId();
        var result = await userService.GetProfile(userId);
        return result.IsSuccess ? Ok(result.Value) : HandleResult(result);

    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest credentials)
    {
        Result<User> result = await userService.Login(credentials.Email, credentials.Password);
        return result.IsSuccess
            ? Ok(await authService.Login(result.Value.Id))
            : HandleResult(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult> Create(CreateUserRequest credentials)
    {
        Result result = await userService.Register(credentials.UserName, credentials.Email, credentials.Password);
        return HandleResult(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshRequest request)
    {
        var result = await authService.Refresh(request.RefreshToken);
        return HandleResult(result); 
    }

    [HttpPost("logout")]
    public async Task<ActionResult<LoginResponse>> Logout([FromBody] RefreshRequest request)
    {
        var result = await authService.Logout(request.RefreshToken);
        return HandleResult(result); 
    }

}

