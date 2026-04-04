using System.Net.Sockets;
using System.Text;
using DefaultNamespace;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace PermissionsApi.Tests;

public sealed class DeviceTcpCommandClientTests
{
    [Fact]
    public async Task SendPowerAsync_WithPromptAwareLogin_SendsExpectedSequence()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var serverTask = Task.Run(async () =>
        {
            using var serverClient = await listener.AcceptTcpClientAsync();
            await using var stream = serverClient.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
            using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true)
            {
                NewLine = "\r\n",
                AutoFlush = true,
            };

            var login = await reader.ReadLineAsync();
            await writer.WriteAsync("ID>");

            var user = await reader.ReadLineAsync();
            await writer.WriteAsync("PW>");

            var password = await reader.ReadLineAsync();
            await writer.WriteAsync(">");

            var command = await reader.ReadLineAsync();

            return new[] { login, user, password, command };
        });

        var options = Options.Create(new DeviceTcpOptions
        {
            Host = "127.0.0.1",
            Port = port,
            TimeoutMs = 2000,
            UseLogin = true,
            LoginCommand = "login",
            SendLoginCommandFirst = true,
            IdPrompt = "ID>",
            PasswordPrompt = "PW>",
            ReadyPrompt = ">",
            UserId = "admin",
            Password = "admin",
            PowerOnCommand = "pset 1 1",
            PromptReadBufferBytes = 64,
            PromptReadMaxChars = 512,
        });

        var client = new DeviceTcpCommandClient(options, NullLogger<DeviceTcpCommandClient>.Instance);

        var result = await client.SendPowerAsync(1, CancellationToken.None);
        var sequence = await serverTask;

        Assert.True(result.Success);
        Assert.Equal("pset 1 1", result.DeviceCommand);
        Assert.Equal(new[] { "login", "admin", "admin", "pset 1 1" }, sequence);
    }

    [Fact]
    public async Task SendPowerAsync_WithPromptFirstMode_SendsCredentialsAfterPrompts()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var serverTask = Task.Run(async () =>
        {
            using var serverClient = await listener.AcceptTcpClientAsync();
            await using var stream = serverClient.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
            using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true)
            {
                NewLine = "\r\n",
                AutoFlush = true,
            };

            await writer.WriteAsync("ID>");
            var user = await reader.ReadLineAsync();

            await writer.WriteAsync("PW>");
            var password = await reader.ReadLineAsync();

            await writer.WriteAsync(">");
            var command = await reader.ReadLineAsync();

            return new[] { user, password, command };
        });

        var options = Options.Create(new DeviceTcpOptions
        {
            Host = "127.0.0.1",
            Port = port,
            TimeoutMs = 2000,
            UseLogin = true,
            LoginCommand = "login",
            SendLoginCommandFirst = false,
            IdPrompt = "ID>",
            PasswordPrompt = "PW>",
            ReadyPrompt = ">",
            UserId = "admin",
            Password = "admin",
            PowerOnCommand = "pset 1 1",
        });

        var client = new DeviceTcpCommandClient(options, NullLogger<DeviceTcpCommandClient>.Instance);

        var result = await client.SendPowerAsync(1, CancellationToken.None);
        var sequence = await serverTask;

        Assert.True(result.Success);
        Assert.Equal(new[] { "admin", "admin", "pset 1 1" }, sequence);
    }

    [Fact]
    public async Task SendPowerAsync_WhenPromptNeverArrives_TimesOut()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        _ = Task.Run(async () =>
        {
            using var serverClient = await listener.AcceptTcpClientAsync();
            await using var stream = serverClient.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
            _ = await reader.ReadLineAsync(); // consume login command and then never send prompts
            await Task.Delay(1000);
        });

        var options = Options.Create(new DeviceTcpOptions
        {
            Host = "127.0.0.1",
            Port = port,
            TimeoutMs = 200,
            UseLogin = true,
            LoginCommand = "login",
            SendLoginCommandFirst = true,
            IdPrompt = "ID>",
            PasswordPrompt = "PW>",
            UserId = "admin",
            Password = "admin",
            PowerOnCommand = "pset 1 1",
        });

        var client = new DeviceTcpCommandClient(options, NullLogger<DeviceTcpCommandClient>.Instance);

        var result = await client.SendPowerAsync(1, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("TCP command timed out.", result.Error);
    }
}

