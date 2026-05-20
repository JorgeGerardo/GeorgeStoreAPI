using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Controllers;

[Route("easydata")]
public class EasyDataController : Controller
{
    [Route("{**entity}")]
    public IActionResult Index(string entity)
    {
        return View();
    }
}