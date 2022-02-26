using System;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using LWSAuthIntegrationTest.Helpers.Containers;

namespace LWSAuthIntegrationTest.Helpers;

public class DockerFixture : IDisposable
{
    private readonly DockerClient _dockerClient;

    public readonly MongoDbContainer MongoDbContainer;

    public DockerFixture()
    {
        _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient();

        MongoDbContainer = new MongoDbContainer(_dockerClient);

        CreateAllContainers().Wait();
        StartAllContainers().Wait();
    }

    private async Task CreateAllContainers()
    {
        await MongoDbContainer.CreateContainerAsync();
    }

    private async Task StartAllContainers()
    {
        await MongoDbContainer.RunContainerAsync();
    }

    private async Task RemoveAllContainers()
    {
        await MongoDbContainer.RemoveContainerAsync();
    }

    public void Dispose()
    {
        RemoveAllContainers().Wait();
        _dockerClient.Dispose();
    }
}