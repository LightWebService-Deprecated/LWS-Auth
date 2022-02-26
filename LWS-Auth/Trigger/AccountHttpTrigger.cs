using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using LWS_Auth.Extension;
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
    private readonly ILogger _logger;

    public AccountHttpTrigger(AccountService accountService, ILogger<AccountHttpTrigger> logger)
    {
        _accountService = accountService;
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
}