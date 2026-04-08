using GeorgeStore.Core;
using GeorgeStore.Data;
using GeorgeStore.Models;
using GeorgeStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserRepository userService, TokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest credentials)
    {
        Result<User> result = await userService.Login(credentials.Email, credentials.Password);
        if (!result.IsSuccess)  
            return BadRequest(new ProblemDetails { Title = result.Error.Tittle, Detail = result.Error.Message });
        
        return Ok(tokenService.GenerateToken(result.Value));
    }

    [HttpPost("register")]
    public async Task<ActionResult> Create(CreateUserRequest credentials)
    {
        Result result = await userService.Register(credentials.UserName, credentials.Email, credentials.Password);
        if (result.IsSuccess)
            return Ok();
        return BadRequest(new ProblemDetails { Title = result.Error.Tittle, Detail = result.Error.Message});
    }

}

public record LoginRequest(string Email, string Password);
public record CreateUserRequest(string UserName, string Email, string Password);