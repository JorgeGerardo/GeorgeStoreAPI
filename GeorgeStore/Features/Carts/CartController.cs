using GeorgeStore.Common;
using GeorgeStore.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Carts;

[Route("api/[controller]")]
[ApiController]
public class CartController(ICartRepository cartRepository) : AuthorizedController
{
    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken ct = default)
    {
        var result = await cartRepository.GetAsync(UserId, ct);
        if (!result.IsSuccess)
            return HandleResult(result);

        var itemsDto = result.Value.Items.Select(CartItemDto.FromEntity).ToList();
        return Ok(new CartDto(result.Value.Id, itemsDto, result.Value.Total));
    }


    [HttpPost]
    public async Task<ActionResult> Add(AddItemRequest request, CancellationToken ct = default)
    {
        Guid userId = HttpContext.User.GetUserId();
        Result result = await cartRepository.AddAsync(userId, request.ProductId, request.Quantity, ct);
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<ActionResult> DecreaseQuantity([FromBody] DecreaseItemDto request)
    {
        Result result = await cartRepository.DecreaseAsync(UserId, request.ProductId);
        return HandleResult(result);
    }

    [HttpDelete("{ProductId}")]
    public async Task<ActionResult> Remove([FromRoute] int ProductId, CancellationToken ct = default)
    {
        Result result = await cartRepository.RemoveAsync(UserId, ProductId, ct);
        return HandleResult(result);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount()
    {
        int count = await cartRepository.CountAsync(UserId);
        return Ok(count);
    }
}

