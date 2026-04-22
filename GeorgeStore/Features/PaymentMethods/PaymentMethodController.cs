using GeorgeStore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GeorgeStore.Features.PaymentMethods;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaymentMethodController(IPaymentMethodRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> Get()
    {
        Guid UserId = HttpContext.User.GetUserId();
        return Ok(await repository.GetAsync(UserId));
    }

    [HttpPost]
    public async Task<ActionResult> Add([FromBody] PaymentMethodCreateDto request)
    {
        Guid userId = HttpContext.User.GetUserId();
        var result = await repository.Add(userId, request);
        return result.IsSuccess
            ? Ok()
            : BadRequest(result.Error);
    }

    [HttpDelete("{paymentMethodId:int}")]
    public async Task<ActionResult> Remove([FromRoute] int paymentMethodId)
    {
        Guid userId = HttpContext.User.GetUserId();
        var result = await repository.Remove(userId, paymentMethodId);
        return result.IsSuccess ? Ok() : NotFound(result.Error);
    }

    [HttpPut("{paymentMethodId:int}")]
    public async Task<ActionResult> UpdateDefaultPaymentMethod(int paymentMethodId)
    {
        Guid userId = HttpContext.User.GetUserId();
        var result = await repository.SetAsDefault(userId, paymentMethodId);
        return result.IsSuccess ? Ok() : Conflict(result.Error);
    }
}
