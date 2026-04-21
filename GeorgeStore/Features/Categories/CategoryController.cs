using GeorgeStore.Common;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Categories;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryRepository categoryRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> Get([FromQuery] QueryParams prms)
    {
        var categories = await categoryRepository.GetAsync(prms);
        return Ok(categories.Select(CategoryDto.FromEntity));
    }
}
