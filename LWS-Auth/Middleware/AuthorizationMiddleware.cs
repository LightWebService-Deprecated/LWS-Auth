using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using LWS_Auth.Attributes;
using LWS_Auth.Extension;
using LWS_Auth.Repository;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace LWS_Auth.Middleware;

[ExcludeFromCodeCoverage]
public class AuthorizationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IAccessTokenRepository _accessTokenRepository;

    public AuthorizationMiddleware(IAccessTokenRepository accessTokenRepository)
    {
        _accessTokenRepository = accessTokenRepository;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var methodInfo = context.GetMethodInfo();
        var attribute = methodInfo.GetCustomAttribute<LwsAuthorizeAttribute>();

        if (attribute != null)
        {
            var targetToken = context.TryGetTokenFromHeaders();
            if (targetToken != null) await AuthorizeUserAsync(context, targetToken, attribute);
        }

        await next(context);
    }

    private async Task AuthorizeUserAsync(FunctionContext context, string token,
        LwsAuthorizeAttribute declaredAttribute)
    {
        var account = await _accessTokenRepository.GetAccessTokenByTokenAsync(token);
        context.SetAccountId(account?.UserId);
    }
}