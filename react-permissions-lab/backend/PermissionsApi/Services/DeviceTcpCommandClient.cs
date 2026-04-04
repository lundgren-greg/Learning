using System.Net.Sockets;
using System.Text;
using DefaultNamespace;
using Microsoft.Extensions.Options;

public interface IDeviceTcpCommandClient
{
    Task<DeviceCommandSendResult> SendPowerAsync(int pwr, CancellationToken cancellationToken);
}

public sealed record DeviceCommandSendResult(bool Success, string ReceivedCommand, string DeviceCommand, string? Error = null);

public sealed class DeviceTcpCommandClient : IDeviceTcpCommandClient
{
    private readonly DeviceTcpOptions _options;

    public DeviceTcpCommandClient(IOptions<DeviceTcpOptions> options)
    {
        _options = options.Value;
    }

    public async Task<DeviceCommandSendResult> SendPowerAsync(int pwr, CancellationToken cancellationToken)
    {
        var receivedCommand = pwr == 1 ? "PWR On(1)" : "PWR Off(0)";
        var mappedDeviceCommand = pwr == 1 ? _options.PowerOnCommand : _options.PowerOffCommand;

        try
        {
            using var client = new TcpClient();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_options.TimeoutMs);

            await client.ConnectAsync(_options.Host, _options.Port, timeoutCts.Token);

            await using var stream = client.GetStream();
            if (_options.UseLogin)
            {
                if (string.IsNullOrWhiteSpace(_options.UserId) || string.IsNullOrWhiteSpace(_options.Password))
                {
                    return new DeviceCommandSendResult(false, receivedCommand, mappedDeviceCommand, "Login is enabled but credentials are missing.");
                }

                await SendLineAsync(stream, _options.LoginCommand, timeoutCts.Token);
                await SendLineAsync(stream, _options.UserId, timeoutCts.Token);
                await SendLineAsync(stream, _options.Password, timeoutCts.Token);
            }

            await SendLineAsync(stream, mappedDeviceCommand, timeoutCts.Token);
            await stream.FlushAsync(timeoutCts.Token);

            return new DeviceCommandSendResult(true, receivedCommand, mappedDeviceCommand);
        }
        catch (OperationCanceledException)
        {
            return new DeviceCommandSendResult(false, receivedCommand, mappedDeviceCommand, "TCP command timed out.");
        }
        catch (Exception ex)
        {
            return new DeviceCommandSendResult(false, receivedCommand, mappedDeviceCommand, ex.Message);
        }
    }

    private static ValueTask SendLineAsync(NetworkStream stream, string command, CancellationToken cancellationToken)
    {
        // netBooter CLI commands are line-oriented and typically expect CRLF.
        var payload = Encoding.UTF8.GetBytes($"{command}\r\n");
        return stream.WriteAsync(payload, cancellationToken);
    }
}

