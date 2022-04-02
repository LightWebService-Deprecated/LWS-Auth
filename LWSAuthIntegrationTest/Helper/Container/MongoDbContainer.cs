using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace LWSAuthIntegrationTest.Helper.Container;

public class MongoDbContainer : DockerImageBase
{
    private string _availablePort;
    protected override string Connections => $"mongodb://root:testPassword@localhost:{_availablePort}";
    public string MongoConnection => Connections;

    public MongoDbContainer(DockerClient dockerClient) : base(dockerClient)
    {
        _availablePort = $"{FindFreePort()}";

        ImageName = "mongo";
        ImageTag = "latest";

        ContainerParameters = new CreateContainerParameters
        {
            Image = FullImageName,
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    ["27017"] = new List<PortBinding> {new() {HostIP = "0.0.0.0", HostPort = _availablePort}}
                }
            },
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                ["27017"] = new()
            },
            Env = new List<string>
            {
                "MONGO_INITDB_ROOT_USERNAME=root",
                "MONGO_INITDB_ROOT_PASSWORD=testPassword"
            },
            Name = "mongodb_integration"
        };
    }

    private int FindFreePort()
    {
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var availablePort = ((IPEndPoint) tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();

        return availablePort;
    }

    public override async Task CreateContainerAsync()
    {
        var containerList =
            await DockerClient.Containers.ListContainersAsync(new ContainersListParameters {All = true});

        var targetContainer = containerList.FirstOrDefault(a => a.Names.Contains("/mongodb_integration"));

        ContainerId = targetContainer == null
            ? await CreateNewContainer()
            : targetContainer.ID;
    }

    public override async Task RunContainerAsync()
    {
        var containerList =
            await DockerClient.Containers.ListContainersAsync(new ContainersListParameters {All = true});
        var targetContainer = containerList.First(a => a.Names.Contains("/mongodb_integration"));

        if (targetContainer.State != "running")
        {
            await DockerClient.Containers.StartContainerAsync(ContainerId, new ContainerStartParameters());
            Thread.Sleep(5000);
        }
        else
        {
            _availablePort = $"{targetContainer.Ports.First().PublicPort}";
        }
    }

    public override async Task RemoveContainerAsync()
    {
        await DockerClient.Containers.StopContainerAsync(ContainerId, new ContainerStopParameters());
        await DockerClient.Containers.RemoveContainerAsync(ContainerId, new ContainerRemoveParameters());
    }

    private async Task<string> CreateNewContainer()
    {
        if (!await CheckImageExists())
        {
            await DockerClient.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = ImageName,
                Tag = ImageTag
            }, new AuthConfig(), new Progress<JSONMessage>());
        }

        return (await DockerClient.Containers.CreateContainerAsync(ContainerParameters)).ID;
    }
}