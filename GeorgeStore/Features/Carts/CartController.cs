using GeorgeStore.Common;
using GeorgeStore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Carts;

[Route("api/[controller]")]
[ApiController]
public class CartController(ICartRepository cartRepository) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<CartDto>> Get(CancellationToken ct = default)
    {
        Guid userId = HttpContext.User.GetUserId();
        var result = await cartRepository.GetAsync(userId, ct);
        if (!result.IsSuccess)
            return NotFound(ProblemDetailFactory.FromError(result.Error));

        var itemsDto = result.Value.Items.Select(i =>
            new CartItemDto(i.Id, i.Item.Name, i.Item.Price, i.Quantity, i.Item.Description, i.Item.Image)
        ).ToList();

        return Ok(new CartDto(itemsDto, result.Value.Total));
    }


    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Add(AddItemRequest request, CancellationToken ct = default)
    {
        Guid userId = HttpContext.User.GetUserId();
        Result result = await cartRepository.AddAsync(userId, request.ProductId, request.Quantity, ct);
        return result.IsSuccess
            ? Ok()
            : BadRequest(ProblemDetailFactory.FromError(result.Error));
    }

    [HttpDelete("{ProductId}")]
    [Authorize]
    public async Task<ActionResult> Remove([FromRoute] int ProductId, CancellationToken ct = default)
    {
        Guid userId = HttpContext.User.GetUserId();
        Result result = await cartRepository.RemoveAsync(userId, ProductId, ct);

        return result.IsSuccess 
            ? Ok() 
            : BadRequest(ProblemDetailFactory.FromError(result.Error));
    }

    [HttpGet("count")]
    [Authorize]
    public async Task<ActionResult<int>> GetCount()
    {
        Guid userId = HttpContext.User.GetUserId();
        int count = await cartRepository.CountAsync(userId);
        return Ok(count);
    }
}
