using System;
using System.Threading.Tasks;
using LWS_Auth.Configuration;
using Microsoft.Azure.Cosmos;

namespace LWS_Auth.Repository;

public static class CosmosClientHelper
{
    public static async Task<CosmosClient> CreateCosmosClient(CosmosConfiguration cosmosConfiguration)
    {
        var cosmosClient = new CosmosClient(cosmosConfiguration.ConnectionString, new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Direct,
            RequestTimeout = TimeSpan.FromMinutes(10),
            AllowBulkExecution = true
        });

        // Create Database if not exists.
        var createdDatabase = await cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosConfiguration.CosmosDbname);
        await createdDatabase.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
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
            Id = cosmosConfiguration.AccountContainerName
        });
        await createdDatabase.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
        {
            PartitionKeyPath = "/UserId",
            Id = cosmosConfiguration.AccessTokenContainerName,
            DefaultTimeToLive = 60 * 30
        });

        return cosmosClient;
    }
}