using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Shared.Base;
using GeorgeStore.Features.Users;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Auth;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IUserService userService, AuthService authService) : ApiControllerBase
{
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
