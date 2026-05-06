using Microsoft.AspNetCore.Mvc;
using GeorgeStore.Features.Shared.Base;

namespace GeorgeStore.Features.PasswordRecovery;

[Route("api/password-recovery")]
[ApiController]
public class PasswordRecoveryController(RecoverPasswordService recoverPasswordService) : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<dynamic>> ForgotPassword(RecoverPassowrdDto request)
    {
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
            ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        await recoverPasswordService.SendRecoverEmailAsync(request, ip, userAgent);
        return Ok();
    }

    [HttpPost("recover")]
    public async Task<ActionResult> Recover(RecoverPassowordInfoRequest request)
    {
        var result = await recoverPasswordService.RecoverAsync(request.Token, request.NewPassowrd);
        return HandleResult(result);
    }
}

