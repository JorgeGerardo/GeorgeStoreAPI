using GeorgeStore.Data;
using GeorgeStore.Models;
using GeorgeStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(IUserRepository userService, TokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest credentials)
    {
        User? user = await userService.Login(credentials.email, credentials.password);
        if (user is null)
            return BadRequest(new ProblemDetails { Detail = "Credentials not valid" });
        
        return Ok(tokenService.GenerateToken(user));
    }

    [HttpPost("register")]
    public async Task<ActionResult> Create(CreateUserRequest credentials)
    {
        bool result = await userService.Register(credentials.userName, credentials.email, credentials.password);
        if (result)
            return Ok();
        return BadRequest();
    }

}

public record LoginRequest(string email, string password);
public record CreateUserRequest(string userName, string email, string password);