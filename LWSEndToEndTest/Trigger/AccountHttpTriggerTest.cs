using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LWS_Auth.Configuration;
using LWS_Auth.Models.Request;
using LWS_Auth.Repository;
using LWSEndToEndTest.Extension;
using Xunit;

namespace LWSEndToEndTest.Trigger;

public class AccountHttpTriggerTest : IDisposable
{
    private readonly DockerHelper _dockerHelper;
    private readonly MongoConfiguration _mongoConfiguration;
    private readonly MongoContext _mongoContext;
    private readonly HttpClient _httpClient;

    public AccountHttpTriggerTest()
    {
        _dockerHelper = new DockerHelper();
        _mongoConfiguration = new MongoConfiguration
        {
            MongoConnection = Environment.GetEnvironmentVariable("E2E_MONGODB_CONNECTION"),
            MongoDbName = Guid.NewGuid().ToString()
        };

        _mongoContext = new MongoContext(_mongoConfiguration);

        _dockerHelper.CreateContainerAsync(new Dictionary<string, object>
        {
            ["IsEncrypted"] = false,
            ["Values"] = new Dictionary<string, object>()
            {
                ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true",
                ["FUNCTIONS_WORKER_RUNTIME"] = "dotnet-isolated",
                ["MongoSection:MongoConnection"] = _mongoConfiguration.MongoConnection,
                ["MongoSection:MongoDbName"] = _mongoConfiguration.MongoDbName
            }
        }).Wait();

        var connectionString = _dockerHelper.RunContainerAsync().GetAwaiter().GetResult();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(connectionString)
        };
    }

    [Fact(DisplayName =
        "POST /api/account (Registration) should respond with 200 OK if corresponding account is new user.")]
    public async Task Is_Registration_Returns_200_When_Account_Is_New()
    {
        // Let
        var registrationRequest = new RegisterRequest
        {
            UserEmail = "testUserEmail@test.com",
            UserPassword = "helloworld",
            UserNickName = "testNickName"
        };

        // Do
        var response = await _httpClient.PostObjectAsync("/api/account", registrationRequest);

        // Check
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName =
        "POST /api/account (Registration) should respond with 400 Bad Request when userEmail is not in format.")]
    public async Task Is_Registration_Returns_400_When_UserEmail_Invalid()
    {
        // Let
        var registrationRequest = new RegisterRequest
        {
            UserEmail = "testUserEmail",
            UserPassword = "helloworld",
            UserNickName = "testNickName"
        };

        // Do
        var response = await _httpClient.PostObjectAsync("/api/account", registrationRequest);

        // Check
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName =
        "POST /api/account (Registration) should respond with 409 CONFLICT when same account registered.")]
    public async Task Is_Registration_Returns_409_When_Same_Account_Registered()
    {
        // Let
        var registrationRequest = new RegisterRequest
        {
            UserEmail = "testUserEmail@test.com",
            UserPassword = "helloworld",
            UserNickName = "testNickName"
        };
        await _httpClient.PostObjectAsync("/api/account", registrationRequest);

        // Do
        var response = await _httpClient.PostObjectAsync("/api/account", registrationRequest);

        // Check
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    public void Dispose()
    {
        _dockerHelper.DestroyContainerAsync().Wait();
        _dockerHelper.Dispose();
        _mongoContext.MongoClient.DropDatabase(_mongoConfiguration.MongoDbName);
        _dockerHelper.Dispose();
    }
}