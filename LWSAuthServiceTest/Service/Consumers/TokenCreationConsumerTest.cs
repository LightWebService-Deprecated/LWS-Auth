using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LWSAuthService.Configuration;
using LWSAuthService.Models;
using LWSAuthService.Repository;
using LWSAuthServiceTest.Helpers;
using LWSEvent.Event.Account;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LWSAuthServiceTest.Service.Consumers;

[Collection("IntegrationCollections")]
public class TokenCreationConsumerTest: IDisposable
{
    private readonly MongoConfiguration _mongoConfiguration;
    private readonly HttpClient _httpClient;
    private readonly WebApplicationFactory<Program> _applicationFactory;

    private readonly IAccountRepository _accountRepository;

    public TokenCreationConsumerTest(IntegrationTestFixture integrationTestFixture)
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

        _accountRepository = _applicationFactory.Services.GetService<IAccountRepository>()!;
    }

    [Fact(DisplayName = "TokenCreationConsumer.Consume: Consume should set namespace token to appropriate account.")]
    public async Task Is_TokenCreationConsumer_Consumes_Message_Set_NameSpaceToken_To_Account()
    {
        // Let
        using var scope = _applicationFactory.Services.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetService<IPublishEndpoint>()!;
        var targetAccount = new Account
        {
            Id = Ulid.NewUlid().ToString(),
            UserEmail = "kangdroid@test.com",
            UserNickName = "KangDroid Test NickName",
            UserPassword = "testPassword",
            AccountState = AccountState.Created,
            AccountRoles = new HashSet<AccountRole> {AccountRole.Admin}
        };
        var message = new TokenCreatedEvent
        {
            AccountId = targetAccount.Id,
            NameSpace = $"{targetAccount.Id}-default",
            NameSpaceToken = Guid.NewGuid().ToString()
        };
        await _accountRepository.CreateAccountAsync(targetAccount);
       
        // Do
        await publishEndpoint.Publish<TokenCreatedEvent>(message);
        Thread.Sleep(1000 * 3); // Let it consume
        
        // Check
        var responseAccount = await _accountRepository.GetAccountByIdAsync(targetAccount.Id);
        Assert.NotNull(responseAccount);
        Assert.Equal(targetAccount.Id, responseAccount.Id);
        Assert.Equal(targetAccount.UserEmail, responseAccount.UserEmail);
        Assert.Equal(targetAccount.UserNickName, responseAccount.UserNickName);
        Assert.Equal(AccountState.Ready, responseAccount.AccountState);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _applicationFactory.Dispose();
    }
}