using GeorgeStore.Features.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Admin.Login;

[Route("c418/login")]
public class LoginController(SignInManager<User> _signInManager, ILogger<LoginController> _logger) : Controller
{
    [HttpGet]
    public IActionResult Index() => View("~/Admin/Login/Index.cshtml");

    [HttpPost]
    public async Task<IActionResult> Index(string userName, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(userName, password, false, false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserName} logged in successfully", userName);
            return Redirect("/f418/easydata");
        }

        _logger.LogWarning(
                "SecurityEvent={SecurityEvent} Failed login attempt for user {UserName}",
                "FailedLogin",
                userName);
        ViewBag.Error = "Invalid credentials";
        return View("~/Admin/Login/Index.cshtml");
    }

}
