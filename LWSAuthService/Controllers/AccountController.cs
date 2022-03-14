using LWSAuthService.Attribute;
using LWSAuthService.Models;
using LWSAuthService.Models.Inner;
using LWSAuthService.Models.Request;
using LWSAuthService.Service;
using Microsoft.AspNetCore.Mvc;

namespace LWSAuthService.Controllers;

using System.Threading.Tasks;

[ApiController]
[Route("/api/account")]
public class AccountHttpTrigger : ControllerBase
{
    private readonly AccountService _accountService;
    private readonly AccessTokenService _accessTokenService;
    private readonly ILogger _logger;

    public AccountHttpTrigger(AccountService accountService, AccessTokenService accessTokenService,
        ILogger<AccountHttpTrigger> logger)
    {
        _accountService = accountService;
        _accessTokenService = accessTokenService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterAsync(RegisterRequest registerRequest)
    {
        var requestValidateResult = registerRequest.ValidateModel();
        if (requestValidateResult.ResultType != ResultType.Success)
        {
            return BadRequest();
        }

        var newAccountResult = await _accountService.CreateNewAccount(registerRequest);

        return newAccountResult.ResultType switch
        {
            ResultType.DataConflicts => Conflict(),
            ResultType.Success => Ok(),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet]
    [LwsAuthorization(TargetAccountRole = AccountRole.User)]
    public async Task<IActionResult> GetAccountInfoAsync()
    {
        var accountGotResult = await _accountService.GetAccountInfoAsync(HttpContext.Items["accountId"].ToString());
        return accountGotResult.ResultType switch
        {
            ResultType.Success => Ok(new
            {
                AccountId = accountGotResult.Result.Id,
                UserNickName = accountGotResult.Result.UserNickName,
                AccountRole = accountGotResult.Result.AccountRoles.First().ToString(),
                FirstLetter = accountGotResult.Result.UserNickName.ToUpper().First()
            }),
            ResultType.DataNotFound => NotFound(accountGotResult),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest loginRequest)
    {
        var loginResult = await _accountService.LoginAccount(loginRequest);
        if (loginResult.ResultType != ResultType.Success)
        {
            return new ObjectResult(loginResult) {StatusCode = StatusCodes.Status403Forbidden};
        }

        return Ok(await _accessTokenService.CreateAccessTokenAsync(loginResult.Result.Id,
            loginResult.Result.AccountRoles));
    }

    [HttpDelete]
    [LwsAuthorization(TargetAccountRole = AccountRole.User)]
    public async Task<IActionResult> DropoutAsync()
    {
        var accountId = HttpContext.Items["accountId"].ToString();
        var removeResult = await _accountService.RemoveAccountAsync(accountId);
        await _accessTokenService.RemoveAccountAccessTokenAsync(accountId);
        return removeResult.ResultType switch
        {
            ResultType.Success => Ok(),
            ResultType.DataNotFound => NotFound(),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
    }
}