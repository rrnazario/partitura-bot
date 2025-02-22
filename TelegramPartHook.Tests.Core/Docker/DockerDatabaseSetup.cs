using Docker.DotNet;
using Docker.DotNet.Models;
using System.Runtime.InteropServices;
using Xunit;

namespace TelegramPartHook.Tests.Core.Docker
{
    public class DockerDatabaseSetup : IAsyncLifetime
    {
        private readonly DockerClient _client;
        private string? _containerId;

        private const string ContainerImageUri = "postgres:14.1-alpine";
        private const string ContainerImageName = "postgres";

        private string? _port { get; set; }
        private string _user { get; set; } = "postgres";
        private string _pwd { get; set; } = "admin";

        public string ConnectionString() =>
            $"User ID={_user};Password={_pwd};Host=localhost;Port={_port};Database=public;;SslMode=disable;TrustServerCertificate=true;CommandTimeout=300;";

        private Uri _uri
        {
            get
            {
                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                if (isWindows)
                {
                    return new("npipe://./pipe/docker_engine");
                }

                var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

                if (isLinux)
                {
                    return new("unix:/var/run/docker.sock");
                }

                throw new Exception("Invalid OS platform");
            }
        }

        public DockerDatabaseSetup() => _client = new DockerClientConfiguration(_uri).CreateClient();

        public async Task InitializeAsync()
        {
            _port = await GetRandomPort();

            await PullImage();
            await StartContainer();
        }

        private async Task StartContainer()
        {
            var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = ContainerImageName,
                ExposedPorts = new Dictionary<string, EmptyStruct>
                {
                    {
                        "5432", default
                    }
                },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { "5432", new List<PortBinding> { new() { HostPort = _port } } }
                    },
                    PublishAllPorts = true,
                },
                Env = new List<string>()
                {
                    $"POSTGRES_USER={_user}",
                    $"POSTGRES_PASSWORD={_pwd}"
                }
            });

            _containerId = response.ID;

            await _client.Containers.StartContainerAsync(_containerId, null);
            const string readyMessage = "database system is ready to accept connections";
            var logs = (stdout: "", "");

            while (!logs.stdout.Contains(readyMessage))
            {
                using var stream = await _client.Containers.GetContainerLogsAsync(_containerId, false, new ContainerLogsParameters{ ShowStdout = true });
                logs = await stream.ReadOutputToEndAsync(CancellationToken.None);
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private async Task<string> GetRandomPort()
        {
            var rnd = new Random(DateTime.Now.Millisecond);

            await Task.Delay(rnd.Next(100));

            var milliseconds = new string(DateTime.Now.ToString("fffffff").Skip(3).ToArray());

            return $"{milliseconds}";
        }

        private async Task PullImage()
        {
            await _client.Images
                .CreateImageAsync(new ImagesCreateParameters
                    {
                        FromImage = ContainerImageUri,
                        Tag = "latest"
                    },
                    new AuthConfig(),
                    new Progress<JSONMessage>());
        }

        public async Task DisposeAsync()
        {
            await KillContainer(_containerId);

            _client.Dispose();
        }

        private async Task KillContainer(string? id)
        {
            if (id != null)
            {
                await _client.Containers.RemoveContainerAsync(id,
                    new ContainerRemoveParameters { Force = true, RemoveVolumes = true });
            }
        }
    }
}