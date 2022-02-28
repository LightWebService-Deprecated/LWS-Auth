using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LWS_Auth.Configuration;
using LWS_Auth.Models;
using LWS_Auth.Models.Request;
using LWS_Auth.Repository;
using LWSEndToEndTest.Extension;
using LWSEndToEndTest.TestData;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Xunit;

namespace LWSEndToEndTest.Trigger;

[Collection("Cosmos")]
public class AccountHttpTriggerTest : IDisposable
{
    private readonly DockerHelper _dockerHelper;
    private readonly CosmosConfiguration _cosmosConfiguration;
    private readonly HttpClient _httpClient;

    public AccountHttpTriggerTest(CosmosFixture cosmosFixture)
    {
        _dockerHelper = new DockerHelper();
        _cosmosConfiguration = cosmosFixture.TestCosmosConfiguration;

        cosmosFixture.CreateAccountContainerAsync(_cosmosConfiguration.AccountContainerName)
            .Wait();
        cosmosFixture.CreateAccessTokenContainerAsync(_cosmosConfiguration.AccessTokenContainerName)
            .Wait();

        _dockerHelper.CreateContainerAsync(new Dictionary<string, object>
        {
            ["IsEncrypted"] = false,
            ["Values"] = new Dictionary<string, object>()
            {
                ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true",
                ["FUNCTIONS_WORKER_RUNTIME"] = "dotnet-isolated",
                ["CosmosSection:ConnectionString"] = _cosmosConfiguration.ConnectionString,
                ["CosmosSection:CosmosDbname"] = _cosmosConfiguration.CosmosDbname,
                ["CosmosSection:AccountContainerName"] = _cosmosConfiguration.AccountContainerName,
                ["CosmosSection:AccessTokenContainerName"] = _cosmosConfiguration.AccessTokenContainerName
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

    [Theory(DisplayName = "POST /api/account/login (Login) should respond with forbidden when login attempt fails")]
    [MemberData(memberName: nameof(AccountTriggerData.LoginFailData), MemberType = typeof(AccountTriggerData))]
    public async Task Is_Login_Returns_Forbidden_When_Login_Failed(AccountLoginFailTestSet testSet)
    {
        if (testSet.Register)
        {
            var registerResult = await _httpClient.PostObjectAsync("/api/account", new RegisterRequest
            {
                UserEmail = testSet.LoginRequest.UserEmail,
                UserPassword = "testSet.LoginRequest.UserPassword",
                UserNickName = Guid.NewGuid().ToString()
            });
            Assert.Equal(HttpStatusCode.OK, registerResult.StatusCode);
        }

        // Do
        var loginResult = await _httpClient.PostObjectAsync("/api/account/login", testSet.LoginRequest);
        Assert.Equal(HttpStatusCode.Forbidden, loginResult.StatusCode);
    }

    [Fact(DisplayName =
        "POST /api/account/login (Login) should respond with 200 OK and access token if login succeeds.")]
    public async Task Is_Login_Returns_OK_With_AccessToken_If_Login_Succeeds()
    {
        // Let
        var registerRequest = new RegisterRequest
        {
            UserEmail = "test@test.com",
            UserPassword = "helloworld",
            UserNickName = "test"
        };
        var response = await _httpClient.PostObjectAsync("/api/account", registerRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loginRequest = new LoginRequest
        {
            UserEmail = registerRequest.UserEmail,
            UserPassword = registerRequest.UserPassword
        };

        // Do
        var loginResponse = await _httpClient.PostObjectAsync("/api/account/login", loginRequest);

        // Check
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var responseBody = JsonConvert.DeserializeObject<AccessToken>(await loginResponse.Content.ReadAsStringAsync());
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Id);
    }

    [Fact(DisplayName =
        "GET /api/account (AccountInfo) should return Unauthorized result if there is no token provided.")]
    public async Task Is_GetAccountInfo_Returns_Unauthorized_If_No_Token_Provided()
    {
        // do
        var response = await _httpClient.GetAsync("/api/account");

        // Check
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "GET /api/account (AccountInfo) should return OK with data if succeeds request.")]
    public async Task Is_GetAccountInfo_Returns_OK_With_Corresponding_Object()
    {
        // Login
        var (registerRequest, accessToken) = await RegisterAndLoginAsync();

        // Do
        _httpClient.DefaultRequestHeaders.Add("X-LWS-AUTH", accessToken.Id);
        var response = await _httpClient.GetAsync("/api/account");
        _httpClient.DefaultRequestHeaders.Clear();

        // Check
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName =
        "DELETE /api/account (Dropout) Should return unauthorized result if there is no token provided")]
    public async Task Is_Dropout_Return_Unauthorized_Result_If_There_Is_No_Token_Provided()
    {
        // Do
        var response = await _httpClient.DeleteAsync("/api/account");

        // Check
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "DELETE /api/account (Dropout) should return OK when dropping out succeeds.")]
    public async Task Is_Dropout_Returns_OK_If_Dropout_Succeeds()
    {
        // Let
        var (registerRequest, accessToken) = await RegisterAndLoginAsync();

        // Do
        _httpClient.DefaultRequestHeaders.Add("X-LWS-AUTH", accessToken.Id);
        var response = await _httpClient.DeleteAsync("/api/account");
        _httpClient.DefaultRequestHeaders.Clear();

        // Check
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public void Dispose()
    {
        _dockerHelper.DestroyContainerAsync().Wait();
        _dockerHelper.Dispose();
    }

    private async Task<(RegisterRequest, AccessToken)> RegisterAndLoginAsync()
    {
        // Let
        var registerRequest = new RegisterRequest
        {
            UserEmail = "test@test.com",
            UserPassword = "helloworld",
            UserNickName = "test"
        };
        var response = await _httpClient.PostObjectAsync("/api/account", registerRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loginRequest = new LoginRequest
        {
            UserEmail = registerRequest.UserEmail,
            UserPassword = registerRequest.UserPassword
        };

        // Do
        var loginResponse = await _httpClient.PostObjectAsync("/api/account/login", loginRequest);

        // Check
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var responseBody = JsonConvert.DeserializeObject<AccessToken>(await loginResponse.Content.ReadAsStringAsync());
        Assert.NotNull(responseBody);
        Assert.NotNull(responseBody.Id);

        return (registerRequest, responseBody);
    }
}