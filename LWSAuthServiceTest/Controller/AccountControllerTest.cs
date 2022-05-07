using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LWSAuthService.Configuration;
using LWSAuthService.Models;
using LWSAuthService.Models.Request;
using LWSAuthServiceTest.Extension;
using LWSAuthServiceTest.Helpers;
using LWSAuthServiceTest.TestData;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace LWSAuthServiceTest.Controller;

[Collection("IntegrationCollections")]
public class AccountControllerTest: IDisposable
{
    private readonly MongoConfiguration _mongoConfiguration;
    private readonly HttpClient _httpClient;
    private readonly WebApplicationFactory<Program> _applicationFactory;

    public AccountControllerTest(IntegrationTestFixture integrationTestFixture)
    {
        _mongoConfiguration = integrationTestFixture.TestMongoConfiguration;
        var dictionaryConfiguration = new Dictionary<string, string>
        {
            ["RabbitMqSection:Host"] = "localhost",
            ["RabbitMqSection:VirtualHost"] = "/",
            ["RabbitMqSection:UserName"] = "guest",
            ["RabbitMqSection:Password"] = "guest"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(dictionaryConfiguration)
            .Build();
        _applicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseConfiguration(configuration);
                builder.ConfigureTestServices(service =>
                {
                    service.AddSingleton(_mongoConfiguration);
                });
            });
        _httpClient = _applicationFactory.CreateClient();
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

    [Fact(DisplayName = "DELETE/api/account (Dropout) should clear access token permanently.")]
    public async Task Is_Dropout_Clears_Access_Token_Permanently()
    {
        // Let
        var (registerRequest, accessToken) = await RegisterAndLoginAsync();
        _httpClient.DefaultRequestHeaders.Add("X-LWS-AUTH", accessToken.Id);
        var dropoutResponse = await _httpClient.DeleteAsync("/api/account");
        _httpClient.DefaultRequestHeaders.Clear();
        Assert.Equal(HttpStatusCode.OK, dropoutResponse.StatusCode);

        // Do(In this phase, user already dropped out and any other request should return 401)
        _httpClient.DefaultRequestHeaders.Add("X-LWS-AUTH", accessToken.Id);
        var response = await _httpClient.GetAsync("/api/account");
        _httpClient.DefaultRequestHeaders.Clear();

        // Check response is 401 unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "GET /api/auth should return unauthorized if header is not defined")]
    public async Task Is_GetAuthorizeToken_Returns_Unauthorized_When_Header_Not_Defined()
    {
        var response = await _httpClient.GetAsync("/api/auth");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "GET /api/auth should return unauthorized if header value is not proper access token.")]
    public async Task Is_GetAuthorizeToken_Returns_Unauthorized_When_Token_Invalid()
    {
        _httpClient.DefaultRequestHeaders.Add("X-LWS-AUTH", "asdfasdfafsd");
        var response = await _httpClient.GetAsync("/api/auth");
        _httpClient.DefaultRequestHeaders.Clear();
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "GET /api/auth should return ok when token is authorized.")]
    public async Task Is_GetAuthorizeTokenAsync_Returns_OK()
    {
        // Let
        var (registerRequest, accessToken) = await RegisterAndLoginAsync();
        _httpClient.DefaultRequestHeaders.Add("X-LWS-AUTH", accessToken.Id);
        var response = await _httpClient.GetAsync("/api/auth");
        _httpClient.DefaultRequestHeaders.Clear();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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

    public void Dispose()
    {
        _httpClient.Dispose();
        _applicationFactory.Dispose();
    }
}