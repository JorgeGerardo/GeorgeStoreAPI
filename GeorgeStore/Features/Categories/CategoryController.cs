using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Categories.Queries.GetCategories;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Categories;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(IQueryDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> Get([FromQuery] QueryParams prms)
    {
        var categories = await dispatcher.Send<GetCategoriesQuery, IEnumerable<CategoryDto>>(new(prms));
        return Ok(categories);
    }
}
