using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using LWS_Auth.Extension;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace LWS_Auth.Trigger;

[ExcludeFromCodeCoverage]
public class MiscHttpTrigger
{
    [Function("MiscHttpTrigger.Alive")]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "misc/alive")]
        HttpRequestData req)
    {
        return req.CreateObjectResult("alive", HttpStatusCode.OK);
    }
}