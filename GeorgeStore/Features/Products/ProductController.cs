using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Products.Queries.GetProducts;
using GeorgeStore.Features.Shared.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Products;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductRepository productRepository, IQueryDispatcher dispatcher) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> Get([FromQuery] QueryParams prms)
    {
        var products = await dispatcher.Send<GetProductsQuery, PagedResult<ProductDto>>(new(prms));
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var result = await productRepository.GetByIdAsync(id);
        return result.IsSuccess 
            ? Ok(ProductDto.FromEntity(result.Value))
            : HandleResult(result);
    }

    //[HttpPost]
    //[Authorize]
    //public async Task<ActionResult> Create(ProductCreateDTO request)
    //{
    //    Result result = await productRepository.CreateAsync(request);
    //    return HandleResult(result);
    //}

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult> Remove(int id)
    {
        var productExist = await productRepository.ExistAsync(id);
        if (!productExist)
            return NotFound(ProductError.Notfound);

        var result = await productRepository.RemoveAsync(id);
        return HandleResult(result);
    }

}
