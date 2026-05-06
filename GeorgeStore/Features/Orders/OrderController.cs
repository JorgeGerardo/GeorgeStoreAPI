using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Orders;

[Route("api/[controller]")]
[ApiController]
public class OrderController(IOrderService orderService) : AuthorizedController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> Get([FromQuery] QueryParams Prms)
    {
        var orders = await orderService.Get(UserId, Prms);
        return Ok(orders);
    }

    [HttpGet("{OrderId:int}")]
    public async Task<ActionResult<Result<OrderDto>>> GetById([FromRoute] int OrderId)
    {
        var result = await orderService.GetByIdAsync(UserId, OrderId);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Purchase([FromBody] BuyRequest request)
    {
        var result = await orderService.Purchase(UserId, request.CartId, request.AddressId, request.PaymentMethodId);
        return HandleResult(result);
    }

    [HttpPost("reorder")]
    public async Task<ActionResult<Result<int>>> Reorder([FromBody] ReorderRequest request)
    {
        var result = await orderService.ReorderAsync(UserId, request);
        return HandleResult(result);
    }

    [HttpGet("reorder/{OrderId:int}")]
    public async Task<ActionResult<ReorderPreview>> ReorderPreview([FromRoute] int OrderId)
    {
        var result = await orderService.PreviewReorder(UserId, OrderId);
        return HandleResult(result);
    }
}

