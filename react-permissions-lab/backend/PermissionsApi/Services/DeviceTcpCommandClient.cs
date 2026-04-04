using System.Net.Sockets;
using System.Text;
using DefaultNamespace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public interface IDeviceTcpCommandClient
{
    Task<DeviceCommandSendResult> SendPowerAsync(int pwr, CancellationToken cancellationToken);
}

public sealed record DeviceCommandSendResult(bool Success, string ReceivedCommand, string DeviceCommand, string? Error = null);

public sealed class DeviceTcpCommandClient : IDeviceTcpCommandClient
{
    private readonly DeviceTcpOptions _options;
    private readonly ILogger<DeviceTcpCommandClient> _logger;

    public DeviceTcpCommandClient(IOptions<DeviceTcpOptions> options, ILogger<DeviceTcpCommandClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<DeviceCommandSendResult> SendPowerAsync(int pwr, CancellationToken cancellationToken)
    {
        var receivedCommand = pwr == 1 ? "PWR On(1)" : "PWR Off(0)";
        var mappedDeviceCommand = pwr == 1 ? _options.PowerOnCommand : _options.PowerOffCommand;

        try
        {
            Debug("Connecting to {host}:{port}", _options.Host, _options.Port);
            using var client = new TcpClient();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_options.TimeoutMs);

            await client.ConnectAsync(_options.Host, _options.Port, timeoutCts.Token);
            Debug("Connected to device.");

            await using var stream = client.GetStream();
            if (_options.UseLogin)
            {
                if (string.IsNullOrWhiteSpace(_options.UserId) || string.IsNullOrWhiteSpace(_options.Password))
                {
                    return new DeviceCommandSendResult(false, receivedCommand, mappedDeviceCommand, "Login is enabled but credentials are missing.");
                }

                if (_options.SendLoginCommandFirst && !string.IsNullOrWhiteSpace(_options.LoginCommand))
                {
                    Debug("Sending login command.");
                    await SendLineAsync(stream, _options.LoginCommand, timeoutCts.Token);
                }
                else
                {
                    Debug("Skipping login command; waiting for ID prompt first.");
                }

                Debug("Waiting for ID prompt: {prompt}", _options.IdPrompt);
                await ReadUntilPromptAsync(stream, _options.IdPrompt, timeoutCts.Token);

                Debug("Sending user id.");
                await SendLineAsync(stream, _options.UserId, timeoutCts.Token);

                Debug("Waiting for password prompt: {prompt}", _options.PasswordPrompt);
                await ReadUntilPromptAsync(stream, _options.PasswordPrompt, timeoutCts.Token);

                Debug("Sending password (masked).");
                await SendLineAsync(stream, _options.Password, timeoutCts.Token);

                if (!string.IsNullOrWhiteSpace(_options.ReadyPrompt))
                {
                    Debug("Waiting for ready prompt: {prompt}", _options.ReadyPrompt);
                    await ReadUntilPromptAsync(stream, _options.ReadyPrompt, timeoutCts.Token);
                }
            }

            Debug("Sending device command: {command}", mappedDeviceCommand);
            await SendLineAsync(stream, mappedDeviceCommand, timeoutCts.Token);
            await stream.FlushAsync(timeoutCts.Token);
            Debug("Device command flushed.");

            return new DeviceCommandSendResult(true, receivedCommand, mappedDeviceCommand);
        }
        catch (OperationCanceledException)
        {
            Debug("TCP operation timed out after {timeoutMs}ms.", _options.TimeoutMs);
            return new DeviceCommandSendResult(false, receivedCommand, mappedDeviceCommand, "TCP command timed out.");
        }
        catch (Exception ex)
        {
            Debug("TCP command failed: {error}", ex.Message);
            return new DeviceCommandSendResult(false, receivedCommand, mappedDeviceCommand, ex.Message);
        }
    }

    private static ValueTask SendLineAsync(NetworkStream stream, string command, CancellationToken cancellationToken)
    {
        // netBooter CLI commands are line-oriented and typically expect CRLF.
        var payload = Encoding.UTF8.GetBytes($"{command}\r\n");
        return stream.WriteAsync(payload, cancellationToken);
    }

    private async Task ReadUntilPromptAsync(NetworkStream stream, string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }

        var buffer = new byte[Math.Max(16, _options.PromptReadBufferBytes)];
        var received = new StringBuilder();

        while (received.Length < Math.Max(256, _options.PromptReadMaxChars))
        {
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                throw new IOException($"Connection closed while waiting for prompt '{prompt}'.");
            }

            var chunk = Encoding.ASCII.GetString(buffer, 0, read);
            received.Append(chunk);

            if (received.ToString().Contains(prompt, StringComparison.OrdinalIgnoreCase))
            {
                Debug("Received prompt {prompt}. Buffer: {buffer}", prompt, SanitizeForLog(received.ToString()));
                return;
            }
        }

        throw new IOException($"Prompt '{prompt}' was not received before read limit.");
    }

    private void Debug(string messageTemplate, params object?[] args)
    {
        _logger.LogInformation(messageTemplate, args);

        if (_options.DebugConsoleOutput)
        {
            var rendered = args.Length == 0
                ? messageTemplate
                : $"{messageTemplate} | {string.Join(", ", args.Select(arg => arg?.ToString() ?? "<null>"))}";
            Console.WriteLine($"[DeviceTcp] {rendered}");
        }
    }

    private static string SanitizeForLog(string input)
    {
        return input
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);
    }
}

