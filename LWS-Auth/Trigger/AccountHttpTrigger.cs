using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LWS_Auth.Attributes;
using LWS_Auth.Extension;
using LWS_Auth.Models;
using LWS_Auth.Models.Inner;
using LWS_Auth.Models.Request;
using LWS_Auth.Service;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace LWS_Auth.Trigger;

[ExcludeFromCodeCoverage]
public class AccountHttpTrigger
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

    [Function("AccountHttpTrigger.RegisterAsync")]
    public async Task<HttpResponseData> RegisterAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "account")]
        HttpRequestData req)
    {
        var registerRequest = req.GetBodyAsAsync<RegisterRequest>();

        var requestValidateResult = registerRequest.ValidateModel();
        if (requestValidateResult.ResultType != ResultType.Success)
        {
            return await req.CreateObjectResult("", HttpStatusCode.BadRequest);
        }

        var newAccountResult = await _accountService.CreateNewAccount(registerRequest);

        return newAccountResult.ResultType switch
        {
            ResultType.DataConflicts => await req.CreateObjectResult("", HttpStatusCode.Conflict),
            ResultType.Success => await req.CreateObjectResult("", HttpStatusCode.OK),
            _ => await req.CreateObjectResult("", HttpStatusCode.InternalServerError)
        };
    }

    [Function("AccountHttpTrigger.GetAccountInfoAsync")]
    [LwsAuthorize(RequestRole = AccountRole.User)]
    public async Task<HttpResponseData> GetAccountInfoAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "account")]
        HttpRequestData req,
        FunctionContext context
    )
    {
        var accountId = context.GetAccountId();
        if (string.IsNullOrEmpty(accountId))
        {
            return await req.CreateObjectResult("", HttpStatusCode.Unauthorized);
        }

        var accountGotResult = await _accountService.GetAccountInfoAsync(accountId);
        return accountGotResult.ResultType switch
        {
            ResultType.Success => await req.CreateObjectResult(new
            {
                UserNickName = accountGotResult.Result.UserNickName,
                AccountRole = accountGotResult.Result.AccountRoles.First().ToString(),
                FirstLetter = accountGotResult.Result.UserNickName.ToUpper().First()
            }, HttpStatusCode.OK),
            ResultType.DataNotFound => await req.CreateObjectResult(accountGotResult, HttpStatusCode.NotFound),
            _ => await req.CreateObjectResult("", HttpStatusCode.InternalServerError)
        };
    }

    [Function("AccountHttpTrigger.LoginAsync")]
    public async Task<HttpResponseData> LoginAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "account/login")]
        HttpRequestData req)
    {
        var loginRequest = req.GetBodyAsAsync<LoginRequest>();
        var loginResult = await _accountService.LoginAccount(loginRequest);
        if (loginResult.ResultType != ResultType.Success)
        {
            return await req.CreateObjectResult(loginResult, HttpStatusCode.Forbidden);
        }

        return await req.CreateObjectResult(await _accessTokenService.CreateAccessTokenAsync(loginResult.Result.Id),
            HttpStatusCode.OK);
    }
}