using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Newtonsoft.Json;

namespace LWSEndToEndTest;

public class DockerHelper : IDisposable
{
    private readonly DockerClient _dockerClient;
    private readonly string _imageName = "lwsauthend";
    private readonly string _imageTag = "latest";
    private readonly CreateContainerParameters _containerParameters;

    private string _containerId;
    private readonly string _localSettingsDirectory;
    private readonly string _availablePort;

    public DockerHelper()
    {
        _localSettingsDirectory = Path.GetTempFileName();
        _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
            .CreateClient();

        _availablePort = $"{FindFreePort()}";
        _containerParameters = new CreateContainerParameters
        {
            Image = $"{_imageName}:{_imageTag}",
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    ["7071"] = new List<PortBinding> {new() {HostIP = "0.0.0.0", HostPort = _availablePort}}
                }
            },
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                ["7071"] = new()
            }
        };
    }

    public async Task CreateContainerAsync(Dictionary<string, object> localSettings)
    {
        // Write Local Settings
        await CreateLocalSettingsAsync(localSettings);

        // Setup Volume
        _containerParameters.HostConfig.Mounts = new List<Mount>
        {
            new()
            {
                Target = "/home/site/wwwroot/local.settings.json",
                Source = _localSettingsDirectory,
                Type = "bind"
            }
        };

        _containerId = (await _dockerClient.Containers.CreateContainerAsync(_containerParameters)).ID;
    }

    public async Task<string> RunContainerAsync()
    {
        var connectionString = $"http://localhost:{_availablePort}";
        await _dockerClient.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(connectionString)
        };

        for (var i = 0; i < 20; i++)
        {
            try
            {
                var response = await httpClient.GetAsync("/api/misc/alive");
                if (response.StatusCode == HttpStatusCode.OK) break;
            }
            catch
            {
            }

            if (i == 19) throw new InvalidOperationException("Server did not started in 10 sec!");
            Thread.Sleep(1000);
        }

        return connectionString;
    }

    public async Task DestroyContainerAsync()
    {
        await _dockerClient.Containers.StopContainerAsync(_containerId, new ContainerStopParameters());
        await _dockerClient.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters());
        File.Delete(_localSettingsDirectory);
    }

    private async Task CreateLocalSettingsAsync(Dictionary<string, object> localSettings)
    {
        await using var fileOpen = File.OpenWrite(_localSettingsDirectory);
        await using var fileWriter = new StreamWriter(fileOpen);
        await fileWriter.WriteAsync(JsonConvert.SerializeObject(localSettings));
    }

    private int FindFreePort()
    {
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var availablePort = ((IPEndPoint) tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();

        return availablePort;
    }

    public void Dispose()
    {
        _dockerClient.Dispose();
    }
}