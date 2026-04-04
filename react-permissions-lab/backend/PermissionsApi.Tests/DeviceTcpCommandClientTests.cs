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

        Assert.True(result.Success, result.Error);
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

        Assert.True(result.Success, result.Error ?? "No error message provided.");
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

    [Fact]
    public async Task SendPowerAsync_WithBannerOnlyStartup_UsesFallbackLoginFlow()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var serverTask = Task.Run(async () =>
        {
            using var serverClient = await listener.AcceptTcpClientAsync();
            await using var stream = serverClient.GetStream();

            var banner = new byte[] { 0xFF, 0xFD, 0x03, 0xFF, 0xFB, 0x01, 0xFF, 0xFE, 0x22 };
            await stream.WriteAsync(banner);
            await stream.WriteAsync(Encoding.ASCII.GetBytes("\r\nSynaccess Inc. Telnet Session V6.1.\r\n"));
            await stream.FlushAsync();

            using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
            var login = await reader.ReadLineAsync();
            var user = await reader.ReadLineAsync();
            var password = await reader.ReadLineAsync();
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
            SendLoginCommandFirst = false,
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

        Assert.True(result.Success, result.Error ?? "No error message provided.");
        Assert.Equal("pset 1 1", result.DeviceCommand);
        Assert.EndsWith("login", sequence[0] ?? string.Empty, StringComparison.Ordinal);
        Assert.Equal("admin", sequence[1]);
        Assert.Equal("admin", sequence[2]);
        Assert.Equal("pset 1 1", sequence[3]);
    }
}

