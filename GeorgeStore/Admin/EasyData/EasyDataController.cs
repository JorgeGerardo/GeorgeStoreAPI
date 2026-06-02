using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Admin.EasyData;

[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "admin")]
[Route("f418/easydata")]
public class EasyDataController : Controller
{
    [Route("{**entity}")]
    [HttpGet]
    public IActionResult Index(string entity) =>
        View("~/Admin/EasyData/Index.cshtml");


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            IdentityConstants.ApplicationScheme);

        return Redirect("/c418/login");
    }
}