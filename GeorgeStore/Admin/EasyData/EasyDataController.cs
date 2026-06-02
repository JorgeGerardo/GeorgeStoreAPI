using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Admin.EasyData;

[Route("f418/easydata")]
public class EasyDataController : Controller
{
    [Route("{**entity}")]
    [HttpGet]
    public IActionResult Index(string entity) =>
        View("~/Admin/EasyData/Index.cshtml");
}