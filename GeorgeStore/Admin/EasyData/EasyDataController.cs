using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Admin.EasyData;

[Route("easydata")]
public class EasyDataController : Controller
{
    [Route("{**entity}")]
    public IActionResult Index(string entity) =>
        View("~/Admin/EasyData/Index.cshtml");
}