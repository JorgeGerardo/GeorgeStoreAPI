using GeorgeStore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Products;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductRepository productRepository) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> Get([FromQuery] QueryParams prms)
    {
        return Ok(await productRepository.GetProducts(prms));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var result = await productRepository.GetById(id);
        return result.IsSuccess 
            ? Ok(ProductDto.FromEntity(result.Value))
            : HandleResult(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create(ProductCreateDTO request)
    {
        Result result = await productRepository.Create(request);
        return result.IsSuccess ? Ok() : HandleResult(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult> Delete(int id)
    {
        var productExist = await productRepository.Exist(id);
        if (!productExist)
            return NotFound(ProductError.Notfound);

        var result = await productRepository.Delete(id);
        return result.IsSuccess ? Ok() : HandleResult(result);
    }

}
