using LWSAuthService.Repository;
using Microsoft.AspNetCore.Mvc;

namespace LWSAuthService.Controllers;

[Route("/api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAccessTokenRepository _accessTokenRepository;

    public AuthController(IAccessTokenRepository accessTokenRepository)
    {
        _accessTokenRepository = accessTokenRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuthorizedAccessTokenAsync()
    {
        var header = HttpContext.Request.Headers["X-LWS-AUTH"].FirstOrDefault();
        if (string.IsNullOrEmpty(header))
        {
            return Unauthorized();
        }

        var accessToken = await _accessTokenRepository.GetAccessTokenByTokenAsync(header);

        if (accessToken == null)
        {
            return Unauthorized();
        }

        return Ok(accessToken);
    }
}