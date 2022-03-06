using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace LWSAuthIntegrationTest.Helper.Container;

public class KafkaContainer : DockerImageBase
{
    private readonly NetworksCreateParameters _kafkaNetworkParameter = new NetworksCreateParameters
    {
        Name = "kafka_network",
        Scope = "local",
        Driver = "bridge"
    };

    private NetworksCreateResponse _networksCreateResponse;
    private CreateContainerResponse _zookeeperContainerResponse;
    private CreateContainerResponse _kafkaContainerResponse;

    public KafkaContainer(DockerClient dockerClient) : base(dockerClient)
    {
    }

    protected override string Connections => "localhost:9095";

    public override async Task CreateContainerAsync()
    {
        // Create Network
        await CreateKafkaNetworkAsync();

        // Create Zookeeper
        await CreateZookeeperContainerAsync();

        // Create Kafka
        await CreateKafkaContainerAsync();
    }

    public override async Task RunContainerAsync()
    {
        await RunZookeeperAsync();
        await RunKafkaAsync();
    }

    public override async Task RemoveContainerAsync()
    {
        // Remove Container
        await RemoveZookeeperContainerAsync();
        await RemoveKafkaContainerAsync();

        // Remove Network
        await RemoveKafkaNetworkAsync();
    }

    private async Task CreateKafkaNetworkAsync()
    {
        _networksCreateResponse = await DockerClient.Networks.CreateNetworkAsync(_kafkaNetworkParameter);
    }

    private async Task RemoveKafkaNetworkAsync()
    {
        await DockerClient.Networks.DeleteNetworkAsync(_networksCreateResponse.ID);
    }

    private async Task CreateZookeeperContainerAsync()
    {
        var fullImageName = "confluentinc/cp-zookeeper:latest";
        var zookeeperContainer = new CreateContainerParameters
        {
            Image = fullImageName,
            Name = "zookeeper_integration",
            HostConfig = new HostConfig
            {
                NetworkMode = _kafkaNetworkParameter.Name
            },
            Env = new List<string>
            {
                "ZOOKEEPER_CLIENT_PORT=2181",
                "ZOOKEEPER_TICK_TIME=2000"
            }
        };
        if (!await CheckImageExists(fullImageName))
        {
            await DockerClient.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = "confluentinc/cp-zookeeper",
                Tag = "latest"
            }, new AuthConfig(), new Progress<JSONMessage>());
        }

        _zookeeperContainerResponse = await DockerClient.Containers.CreateContainerAsync(zookeeperContainer);
    }

    private async Task RunZookeeperAsync()
    {
        await DockerClient.Containers.StartContainerAsync(_zookeeperContainerResponse.ID,
            new ContainerStartParameters());
        Thread.Sleep(3000);
    }

    private async Task RemoveZookeeperContainerAsync()
    {
        await DockerClient.Containers.StopContainerAsync(_zookeeperContainerResponse.ID,
            new ContainerStopParameters());
        await DockerClient.Containers.RemoveContainerAsync(_zookeeperContainerResponse.ID,
            new ContainerRemoveParameters());
    }

    private async Task CreateKafkaContainerAsync()
    {
        var kafkaContainer = new CreateContainerParameters
        {
            Image = "confluentinc/cp-kafka:latest",
            Name = "kafka_integration",
            HostConfig = new HostConfig
            {
                NetworkMode = _kafkaNetworkParameter.Name,
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    ["9092"] = new List<PortBinding> {new() {HostIP = "0.0.0.0", HostPort = "9092"}}
                }
            },
            Env = new List<string>
            {
                "KAFKA_BROKER_ID=1",
                "KAFKA_ZOOKEEPER_CONNECT=zookeeper_integration:2181",
                "KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://kafka_integration:29092,PLAINTEXT_HOST://localhost:9092",
                "KAFKA_LISTENER_SECURITY_PROTOCOL_MAP=PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT",
                "KAFKA_INTER_BROKER_LISTENER_NAME=PLAINTEXT",
                "KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1"
            },
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                ["9092"] = new()
            }
        };

        if (!await CheckImageExists("confluentinc/cp-kafka:latest"))
        {
            await DockerClient.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = "confluentinc/cp-kafka",
                Tag = "latest"
            }, new AuthConfig(), new Progress<JSONMessage>());
        }

        _kafkaContainerResponse = await DockerClient.Containers.CreateContainerAsync(kafkaContainer);
    }

    private async Task RunKafkaAsync()
    {
        await DockerClient.Containers.StartContainerAsync(_kafkaContainerResponse.ID,
            new ContainerStartParameters());
        Thread.Sleep(2000);
    }

    private async Task RemoveKafkaContainerAsync()
    {
        await DockerClient.Containers.StopContainerAsync(_kafkaContainerResponse.ID,
            new ContainerStopParameters());
        await DockerClient.Containers.RemoveContainerAsync(_kafkaContainerResponse.ID,
            new ContainerRemoveParameters());
    }
}