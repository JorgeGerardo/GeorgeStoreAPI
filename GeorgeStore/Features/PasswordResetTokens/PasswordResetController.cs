using Microsoft.AspNetCore.Mvc;
using GeorgeStore.Common;

namespace GeorgeStore.Features.PasswordResetTokens;

[Route("api/[controller]")]
[ApiController]
public class PasswordResetController(RecoverPasswordService recoverPasswordService) : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<dynamic>> ForgotPassword(RecoverPassowrdDto request)
    {
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
            ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        await recoverPasswordService.SendRecoverEmail(request, ip, userAgent);
        return Ok();
    }

    [HttpPost("recover")]
    public async Task<ActionResult> Recover(RecoverPassowordInfoRequest request)
    {
        var result = await recoverPasswordService.Recover(request.Token, request.NewPassowrd);
        return HandleResult(result);
    }
}

