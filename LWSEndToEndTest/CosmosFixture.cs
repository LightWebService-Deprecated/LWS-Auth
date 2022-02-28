using System;
using System.Threading.Tasks;
using LWS_Auth.Configuration;
using Microsoft.Azure.Cosmos;

namespace LWSEndToEndTest;

public class CosmosFixture : IDisposable
{
    public readonly CosmosClient CosmosClient;

    private readonly string _connectionString;
    public const string DatabaseName = "test-db";

    public CosmosConfiguration TestCosmosConfiguration => new()
    {
        ConnectionString = _connectionString,
        CosmosDbname = DatabaseName,
        AccountContainerName = $"Account{Guid.NewGuid().ToString()}",
        AccessTokenContainerName = $"AccessToken{Guid.NewGuid().ToString()}"
    };

    public CosmosFixture()
    {
        _connectionString = Environment.GetEnvironmentVariable("INTEGRATION_COSMOS_CONNECTION")
                            ?? throw new NullReferenceException("Cosmos Connection is not defined!");
        CosmosClient = new CosmosClient(_connectionString, new CosmosClientOptions
        {
            AllowBulkExecution = true,
            ConnectionMode = ConnectionMode.Direct
        });

        CosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseName).Wait();
    }

    public async Task CreateAccountContainerAsync(string accountContainerName)
    {
        await CosmosClient.GetDatabase(DatabaseName).CreateContainerIfNotExistsAsync(new ContainerProperties
        {
            UniqueKeyPolicy = new UniqueKeyPolicy
            {
                UniqueKeys =
                {
                    new UniqueKey
                    {
                        Paths = {"/UserEmail"}
                    }
                }
            },
            PartitionKeyPath = "/UserEmail",
            Id = accountContainerName
        });
    }

    public async Task CreateAccessTokenContainerAsync(string accessTokenName)
    {
        await CosmosClient.GetDatabase(DatabaseName).CreateContainerIfNotExistsAsync(new ContainerProperties
        {
            PartitionKeyPath = "/UserId",
            Id = accessTokenName,
            DefaultTimeToLive = 60 * 30
        });
    }

    public void Dispose()
    {
        CosmosClient.GetDatabase(DatabaseName)
            .DeleteAsync()
            .Wait();
        CosmosClient.Dispose();
    }
}