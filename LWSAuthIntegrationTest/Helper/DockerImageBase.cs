using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace LWSAuthIntegrationTest.Helper;

public abstract class DockerImageBase
{
    protected string ImageName;
    protected string ImageTag;
    protected string ContainerId;
    protected CreateContainerParameters ContainerParameters;
    protected readonly DockerClient DockerClient;
    protected abstract string Connections { get; }

    // Full Image Name
    protected string FullImageName => $"{ImageName}:{ImageTag}";

    protected DockerImageBase(DockerClient dockerClient)
    {
        DockerClient = dockerClient;
    }

    protected async Task<bool> CheckImageExists()
    {
        var list = await DockerClient.Images.ListImagesAsync(new ImagesListParameters
        {
            All = true
        });
        return list.Any(a => a.RepoTags.Contains(FullImageName));
    }

    protected async Task<bool> CheckImageExists(string fullImageName)
    {
        var list = await DockerClient.Images.ListImagesAsync(new ImagesListParameters
        {
            All = true
        });
        return list.Any(a => a.RepoTags.Contains(fullImageName));
    }

    public abstract Task CreateContainerAsync();
    public abstract Task RunContainerAsync();
    public abstract Task RemoveContainerAsync();
}