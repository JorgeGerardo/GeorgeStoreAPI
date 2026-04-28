using GeorgeStore.Common;
using GeorgeStore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.Orders;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> Get([FromQuery] QueryParams Prms)
    {
        Guid UserId = HttpContext.User.GetUserId();
        var orders = await orderService.Get(UserId, Prms);
        return Ok(orders);
    }

    [HttpGet("{OrderId:int}")]
    public async Task<ActionResult<Result<OrderDto>>> GetById([FromRoute] int OrderId)
    {
        Guid UserId = HttpContext.User.GetUserId();
        var result = await orderService.GetById(UserId ,OrderId);
        return result.IsSuccess 
            ? Ok(result.Value) 
            : NotFound(ProblemDetailFactory.FromError(result.Error));
    }

    [HttpPost]
    public async Task<ActionResult<int>> Buy([FromBody] BuyRequest request)
    {
        Guid UserId = HttpContext.User.GetUserId();
        var result = await orderService.Buy(UserId, request.CartId, request.AddressId, request.PaymentMethodId);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500);
    }

    //TODO: Reorder endpoint missing...
    [HttpPost("{OrderId:int}")]
    public Task<ActionResult<Result<OrderDto>>> Reorder([FromRoute] int OrderId)
    {
        throw new NotImplementedException();
    }
}
