using GeorgeStore.Data;
using GeorgeStore.Models;
using GeorgeStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductRepository productRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Get([FromQuery] QueryParams prms)
    {
        return Ok(await productRepository.GetProducts(prms));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetById(int id)
    {
        return Ok(await productRepository.GetById(id));
    }

    [HttpPost]
    public async Task<ActionResult> Create(ProductCreateDTO request)
    {
        bool result = await productRepository.Create(request);
        return result is true ? Ok() : BadRequest();
    }

    [HttpGet("delete/{id}")]
    public async Task<ActionResult<IEnumerable<Product>>> Delete(int id)
    {
        return Ok(await productRepository.Delete(id));
    }



}
