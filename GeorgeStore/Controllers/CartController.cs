using GeorgeStore.Data;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartController(ICartService cartService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Add(AddProductRequest request)
    {
        bool result = await cartService.Add(request.ProductId, request.Quantity);
        return result ? Ok(result) : BadRequest();
    }
}


public record AddProductRequest(string ProductId, uint Quantity);