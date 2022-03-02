using LWSAuthService.Models;
using LWSAuthService.Models.Inner;
using LWSAuthService.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LWSAuthService.Attribute;

public class LwsAuthorization : ActionFilterAttribute
{
    public AccountRole TargetAccountRole { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var httpContext = context.HttpContext;
        var accessTokenRepository = httpContext.RequestServices.GetService<IAccessTokenRepository>();
        if (!httpContext.Request.Headers.TryGetValue("X-LWS-AUTH", out var token))
        {
            context.Result = new UnauthorizedObjectResult(new InternalCommunication<object>
            {
                ResultType = ResultType.InvalidRequest,
                Message = "This API needs to be logged-in Please login!"
            });
        }

        var account = accessTokenRepository.GetAccessTokenByTokenAsync(token)
            .GetAwaiter().GetResult();
        if (account?.Roles.Contains(TargetAccountRole) == true)
        {
            httpContext.Items.Add("accountId", account.UserId);
        }
        else
        {
            context.Result = new UnauthorizedObjectResult(new InternalCommunication<object>
            {
                ResultType = ResultType.InvalidRequest,
                Message = "This API needs to be logged-in Please login!"
            });
        }

        base.OnActionExecuting(context);
    }
}