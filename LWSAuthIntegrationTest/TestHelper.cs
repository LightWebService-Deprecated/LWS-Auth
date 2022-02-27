using System;
using LWS_Auth.Configuration;
using LWS_Auth.Repository;
using Microsoft.Azure.Cosmos;

namespace LWSAuthIntegrationTest;

public class CosmosDatabaseHelper : IDisposable
{
    public readonly CosmosClient CosmosClient;
    public readonly CosmosConfiguration CosmosConfiguration;

    protected CosmosDatabaseHelper()
    {
        var cosmosConnectionString = Environment.GetEnvironmentVariable("INTEGRATION_COSMOS_CONNECTION")
                                     ?? throw new NullReferenceException("Cosmos Connection is not defined!");

        CosmosConfiguration = new CosmosConfiguration
        {
            ConnectionString = cosmosConnectionString,
            CosmosDbname = Guid.NewGuid().ToString(),
            AccessTokenContainerName = Guid.NewGuid().ToString(),
            AccountContainerName = Guid.NewGuid().ToString()
        };

        CosmosClient = CosmosClientHelper.CreateCosmosClient(CosmosConfiguration)
            .GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        CosmosClient.GetDatabase(CosmosConfiguration.CosmosDbname)
            .DeleteAsync()
            .Wait();
    }
}