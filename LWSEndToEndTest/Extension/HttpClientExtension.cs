using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LWSEndToEndTest.Extension;

public static class HttpClientExtension
{
    public static async Task<HttpResponseMessage> PostObjectAsync(this HttpClient httpClient, string endpoint,
        object body)
    {
        return await httpClient.PostAsync(endpoint,
            new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));
    }
}