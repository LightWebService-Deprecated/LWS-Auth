using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using LWS_Auth.Attributes;
using LWS_Auth.Extension;
using LWS_Auth.Repository;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Newtonsoft.Json;

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
            var targetToken = TryGetTokenFromHeaders(context);
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

    private static string TryGetTokenFromHeaders(FunctionContext context)
    {
        if (!context.BindingContext.BindingData.TryGetValue("Header", out var headerObject)) return null;
        if (headerObject is not string headerString) return null;

        var headerDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(headerString);
        if (headerDictionary == null) return null;

        var token = headerDictionary.GetValueOrDefault("X-LWS-AUTH");
        return token;
    }
}