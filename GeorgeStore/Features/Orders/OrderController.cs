using GeorgeStore.Common.Core.Interfaces;
using GeorgeStore.Common.Shared;
using GeorgeStore.Features.Orders.Queries.Get;
using GeorgeStore.Features.Orders.Queries.GetById;
using GeorgeStore.Features.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Orders;

[Route("api/[controller]")]
[ApiController]
public class OrderController(IOrderService orderService, IQueryDispatcher dispatcher) : AuthorizedController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> Get([FromQuery] QueryParams Prms)
    {
        var orders = await dispatcher.Send<GetOrdersQuery, PagedResult<OrderDto>>(new(UserId, Prms));
        return Ok(orders);
    }

    [HttpGet("{OrderId:int}")]
    public async Task<ActionResult<Result<OrderDto>>> GetById([FromRoute] int OrderId)
    {
        var result = await dispatcher.Send<GetOrderByIdQuery, Result<OrderDto>>(new(UserId, OrderId));
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

