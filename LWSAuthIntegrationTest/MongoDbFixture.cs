using System;
using System.Runtime.CompilerServices;
using Confluent.Kafka;
using Docker.DotNet;
using LWSAuthIntegrationTest.Helper.Container;
using LWSAuthService.Configuration;
using LWSAuthService.Repository;
using MongoDB.Driver;

namespace LWSAuthIntegrationTest;

public class MongoDbFixture : IDisposable
{
    public MongoContext MongoContext => new MongoContext(TestMongoConfiguration);
    private const string DatabaseName = "test-db";

    // Container
    private readonly DockerClient _dockerClient;
    private readonly MongoDbContainer _mongoDbContainer;
    private readonly KafkaContainer _kafkaContainer;
    private readonly string _connectionString;

    public MongoConfiguration TestMongoConfiguration => new()
    {
        ConnectionString = _connectionString,
        DatabaseName = DatabaseName,
        AccountCollectionName = Ulid.NewUlid().ToString(),
        AccessTokenCollectionName = Ulid.NewUlid().ToString()
    };

    public ProducerConfig TestProducerConfig = new()
    {
        BootstrapServers = "localhost:9092",
        ClientId = "IntegrationDevelopmentAuthProducerClient",
    };

    public MongoDbFixture()
    {
        _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient();
        _mongoDbContainer = new MongoDbContainer(_dockerClient);
        _kafkaContainer = new KafkaContainer(_dockerClient);

        _mongoDbContainer.CreateContainerAsync().Wait();
        _mongoDbContainer.RunContainerAsync().Wait();
        _kafkaContainer.CreateContainerAsync().Wait();
        _kafkaContainer.RunContainerAsync().Wait();

        _connectionString = _mongoDbContainer.MongoConnection;
    }

    public void Dispose()
    {
        _mongoDbContainer.RemoveContainerAsync().Wait();
        _kafkaContainer.RemoveContainerAsync().Wait();
        _dockerClient.Dispose();
    }
}