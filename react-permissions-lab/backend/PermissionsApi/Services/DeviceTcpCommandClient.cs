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
    private const byte Iac = 255;
    private const byte Dont = 254;
    private const byte Do = 253;
    private const byte Wont = 252;
    private const byte Will = 251;

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

                var sentLoginCommand = false;
                if (_options.SendLoginCommandFirst && !string.IsNullOrWhiteSpace(_options.LoginCommand))
                {
                    Debug("Sending login command.");
                    await SendLineAsync(stream, _options.LoginCommand, timeoutCts.Token);
                    sentLoginCommand = true;
                }
                else
                {
                    Debug("Skipping upfront login command; inspecting session header first.");
                }

                var sessionHeader = await ReadSessionWindowAsync(stream, timeoutCts.Token, Math.Min(1000, _options.TimeoutMs));
                var sawIdPrompt = ContainsPrompt(sessionHeader.Text, _options.IdPrompt);
                if (!sawIdPrompt && !sentLoginCommand && !string.IsNullOrWhiteSpace(_options.LoginCommand))
                {
                    Debug("ID prompt not detected in initial header; sending fallback login command.");
                    await SendLineAsync(stream, _options.LoginCommand, timeoutCts.Token);
                    sentLoginCommand = true;
                    var afterLogin = await ReadSessionWindowAsync(stream, timeoutCts.Token, 300);
                    sessionHeader = sessionHeader with { Text = sessionHeader.Text + afterLogin.Text };
                    sawIdPrompt = ContainsPrompt(sessionHeader.Text, _options.IdPrompt);
                }

                if (string.IsNullOrWhiteSpace(sessionHeader.Text))
                {
                    throw new OperationCanceledException("Device did not return any session header or prompt after login bootstrap.");
                }

                if (sawIdPrompt)
                {
                    Debug("Detected ID prompt: {prompt}", _options.IdPrompt);
                }
                else
                {
                    Debug("ID prompt not found; treating banner/header as login-ready state.");
                }

                Debug("Sending user id.");
                await SendLineAsync(stream, _options.UserId, timeoutCts.Token, redactForLog: true);

                var afterUserReadMs = sawIdPrompt ? 500 : 150;
                var afterUser = await ReadSessionWindowAsync(stream, timeoutCts.Token, afterUserReadMs);
                var sawPasswordPrompt = ContainsPrompt(afterUser.Text, _options.PasswordPrompt);
                if (sawPasswordPrompt)
                {
                    Debug("Detected password prompt: {prompt}", _options.PasswordPrompt);
                }
                else
                {
                    Debug("Password prompt not found after user id; sending password anyway.");
                }

                Debug("Sending password (masked).");
                await SendLineAsync(stream, _options.Password, timeoutCts.Token, redactForLog: true);

                if (!string.IsNullOrWhiteSpace(_options.ReadyPrompt))
                {
                    var afterPasswordReadMs = sawIdPrompt ? 500 : 150;
                    var afterPassword = await ReadSessionWindowAsync(stream, timeoutCts.Token, afterPasswordReadMs);
                    if (ContainsPrompt(afterPassword.Text, _options.ReadyPrompt))
                    {
                        Debug("Detected ready prompt: {prompt}", _options.ReadyPrompt);
                    }
                    else
                    {
                        Debug("Ready prompt not detected after password; proceeding with command send.");
                    }
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

    private async ValueTask SendLineAsync(NetworkStream stream, string command, CancellationToken cancellationToken, bool redactForLog = false)
    {
        // netBooter CLI commands are line-oriented and typically expect CRLF.
        var payload = Encoding.UTF8.GetBytes($"{command}\r\n");
        var commandForLog = redactForLog ? "<redacted>" : command;
        Debug("TX ({bytes} bytes): {command}", payload.Length, commandForLog);
        await stream.WriteAsync(payload, cancellationToken);
    }

    private async Task<SessionWindowRead> ReadSessionWindowAsync(NetworkStream stream, CancellationToken cancellationToken, int windowTimeoutMs)
    {
        var text = new StringBuilder();
        var receivedAny = false;
        var buffer = new byte[Math.Max(16, _options.PromptReadBufferBytes)];

        using var readWindowCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        readWindowCts.CancelAfter(Math.Max(50, windowTimeoutMs));

        while (text.Length < Math.Max(256, _options.PromptReadMaxChars))
        {
            TelnetChunkRead read;
            try
            {
                read = await ReadChunkAsync(stream, buffer, readWindowCts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (read.BytesRead == 0)
            {
                if (receivedAny)
                {
                    break;
                }

                throw new IOException("Connection closed while waiting for device session data.");
            }

            receivedAny = true;
            if (read.ReplyBytes.Length > 0)
            {
                await stream.WriteAsync(read.ReplyBytes, cancellationToken);
                Debug("TX telnet negotiation bytes: {bytesHex}", BytesToHex(read.ReplyBytes));
            }

            if (!string.IsNullOrEmpty(read.TextChunk))
            {
                text.Append(read.TextChunk);
            }

            Debug(
                "RX ({byteCount} bytes) raw={rawHex} text='{text}'",
                read.BytesRead,
                read.RawHex,
                SanitizeForLog(read.TextChunk));

            if (!stream.DataAvailable)
            {
                if (!string.IsNullOrWhiteSpace(read.TextChunk))
                {
                    break;
                }

                continue;
            }
        }

        var textValue = text.ToString();
        Debug("Session window complete. text='{text}'", SanitizeForLog(textValue));
        return new SessionWindowRead(textValue);
    }

    private static bool ContainsPrompt(string content, string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            return false;
        }

        return content.Contains(prompt, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<TelnetChunkRead> ReadChunkAsync(NetworkStream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var read = await stream.ReadAsync(buffer, cancellationToken);
        if (read == 0)
        {
            return new TelnetChunkRead(0, string.Empty, string.Empty, Array.Empty<byte>());
        }

        var textBytes = new List<byte>(read);
        var negotiationReply = new List<byte>();
        var i = 0;
        while (i < read)
        {
            if (buffer[i] == Iac)
            {
                if (i + 1 >= read)
                {
                    break;
                }

                var command = buffer[i + 1];
                if (command == Iac)
                {
                    textBytes.Add(Iac);
                    i += 2;
                    continue;
                }

                if (i + 2 < read)
                {
                    var option = buffer[i + 2];
                    if (command == Will)
                    {
                        negotiationReply.Add(Iac);
                        negotiationReply.Add(Dont);
                        negotiationReply.Add(option);
                    }
                    else if (command == Do)
                    {
                        negotiationReply.Add(Iac);
                        negotiationReply.Add(Wont);
                        negotiationReply.Add(option);
                    }

                    i += 3;
                    continue;
                }

                break;
            }

            textBytes.Add(buffer[i]);
            i++;
        }

        var text = Encoding.ASCII.GetString(textBytes.ToArray());
        var rawHex = BytesToHex(buffer.AsSpan(0, read));
        return new TelnetChunkRead(read, text, rawHex, negotiationReply.ToArray());
    }

    private static string BytesToHex(ReadOnlySpan<byte> bytes)
    {
        return Convert.ToHexString(bytes);
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

    private readonly record struct TelnetChunkRead(int BytesRead, string TextChunk, string RawHex, byte[] ReplyBytes);

    private readonly record struct SessionWindowRead(string Text);
}

