using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace LWS_Auth.Extension;

[ExcludeFromCodeCoverage]
public static class HttpResponseDataExtension
{
    public static async Task<HttpResponseData> CreateObjectResult(this HttpRequestData requestData, object body,
        HttpStatusCode statusCode)
    {
        var response = requestData.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(body);
        response.StatusCode = statusCode;

        return response;
    }

    public static TBody GetBodyAsAsync<TBody>(this HttpRequestData requestData)
    {
        var serializer = new JsonSerializer();
        using var streamReader = new StreamReader(requestData.Body);
        using var jsonReader = new JsonTextReader(streamReader);

        return serializer.Deserialize<TBody>(jsonReader);
    }
}