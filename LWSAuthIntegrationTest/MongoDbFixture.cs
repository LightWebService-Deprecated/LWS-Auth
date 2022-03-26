using System;
using Docker.DotNet;
using LWSAuthIntegrationTest.Helper.Container;
using LWSAuthService.Configuration;
using LWSAuthService.Repository;

namespace LWSAuthIntegrationTest;

public class MongoDbFixture : IDisposable
{
    public MongoContext MongoContext => new MongoContext(TestMongoConfiguration);
    private const string DatabaseName = "test-db";

    // Container
    private readonly DockerClient _dockerClient;
    private readonly MongoDbContainer _mongoDbContainer;
    private readonly string _connectionString;

    public MongoConfiguration TestMongoConfiguration => new()
    {
        ConnectionString = _connectionString,
        DatabaseName = DatabaseName,
        AccountCollectionName = Ulid.NewUlid().ToString(),
        AccessTokenCollectionName = Ulid.NewUlid().ToString()
    };

    public MongoDbFixture()
    {
        _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient();
        _mongoDbContainer = new MongoDbContainer(_dockerClient);

        _mongoDbContainer.CreateContainerAsync().Wait();
        _mongoDbContainer.RunContainerAsync().Wait();

        _connectionString = _mongoDbContainer.MongoConnection;
    }

    public void Dispose()
    {
        _mongoDbContainer.RemoveContainerAsync().Wait();
        _dockerClient.Dispose();
    }
}