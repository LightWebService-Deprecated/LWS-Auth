using LWSAuthService.Repository;
using Microsoft.AspNetCore.Mvc;

namespace LWSAuthService.Controllers;

[Route("/api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAccessTokenRepository _accessTokenRepository;
    private readonly ILogger _logger;

    public AuthController(IAccessTokenRepository accessTokenRepository, ILogger<AuthController> logger)
    {
        _accessTokenRepository = accessTokenRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuthorizedAccessTokenAsync()
    {
        var header = HttpContext.Request.Headers["X-LWS-AUTH"].FirstOrDefault();
        if (string.IsNullOrEmpty(header))
        {
            _logger.LogInformation("Auth Header is empty!");
            return Unauthorized();
        }

        var accessToken = await _accessTokenRepository.GetAccessTokenByTokenAsync(header);

        if (accessToken == null)
        {
            _logger.LogInformation("Access Token is null: {token}", header);
            return Unauthorized();
        }

        return Ok(accessToken);
    }
}