using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Docker.DotNet;
using Docker.DotNet.Models;

using ProductContext.Integration.Tests.Logging;
using ProductContext.Integration.Tests.Logging.LogProviders;

namespace ProductContext.Integration.Tests
{
    public static class DockerHelper
    {
        private static readonly ILog Log = LogProvider.For<DockerClient>();
        private static readonly DockerClient DockerClient = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();

        public static async Task<IDisposable> StartContainerAsync(
            string image,
            IReadOnlyCollection<int> ports = null,
            IReadOnlyDictionary<string, string> environment = null)
        {
            using (var ts = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
            {
                var env = environment
                          ?.OrderBy(kv => kv.Key)
                          .Select(kv => $"{kv.Key}={kv.Value}")
                          .ToList();

                Log.Info($"Starting container with image '{image}'");

                Log.Info($"Pulling image {image}");
                var imageAndTag = image.Split(':');
                await DockerClient.Images.CreateImageAsync(
                                      new ImagesCreateParameters
                                      {
                                          FromImage = imageAndTag[0],
                                          Tag = imageAndTag.Length > 1 ? imageAndTag[1] : null
                                      },
                                      null,
                                      new Progress<JSONMessage>(m =>
                                      {
                                          Log.Trace($"{m.ProgressMessage} ({m.ID})");
                                      }),
                                      ts.Token)
                                  .ConfigureAwait(false);
                while (true)
                {
                    IList<ImagesListResponse> imagesListResponses = await DockerClient.Images.ListImagesAsync(new ImagesListParameters(), ts.Token).ConfigureAwait(false);
                    if (imagesListResponses.Any(i =>
                        i.RepoTags != null &&
                        i.RepoTags.Any(t => string.Equals(t, image, StringComparison.OrdinalIgnoreCase))))
                    {
                        break;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                CreateContainerResponse createContainerResponse = await DockerClient.Containers.CreateContainerAsync(
                                                                                        new CreateContainerParameters(
                                                                                            new Config
                                                                                            {
                                                                                                Image = image,
                                                                                                Env = env,
                                                                                                ExposedPorts = ports?
                                                                                                    .ToDictionary(p => $"{p}/tcp", p => new EmptyStruct())
                                                                                            })
                                                                                        {
                                                                                            HostConfig = new HostConfig
                                                                                            {
                                                                                                PortBindings = ports?.ToDictionary(
                                                                                                    p => $"{p}/tcp",
                                                                                                    p => (IList<PortBinding>)new List<PortBinding>
                                                                                                    {
                                                                                                        new PortBinding { HostPort = p.ToString() }
                                                                                                    })
                                                                                            }
                                                                                        },
                                                                                        ts.Token)
                                                                                    .ConfigureAwait(false);
                Log.Info($"Successfully created container '{createContainerResponse.ID}'");

                await DockerClient.Containers.StartContainerAsync(
                                      createContainerResponse.ID,
                                      new ContainerStartParameters(),
                                      ts.Token)
                                  .ConfigureAwait(false);
                Log.Info($"Successfully started container '{createContainerResponse.ID}'");

                return new DisposableAction(() => StopContainer(createContainerResponse.ID));
            }
        }

        private static void StopContainer(string id)
        {
            try
            {
                DockerClient.Containers.StopContainerAsync(
                    id,
                    new ContainerStopParameters
                    {
                        WaitBeforeKillSeconds = 5
                    }).Wait();
                Log.Info($"Stopped container {id}");

                DockerClient.Containers.RemoveContainerAsync(
                    id,
                    new ContainerRemoveParameters
                    {
                        Force = true
                    }).Wait();
                Log.Info($"Removed container {id}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
