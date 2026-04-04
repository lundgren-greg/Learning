// ---------------------------------------------------------------------------
// DeviceTcpCliTester – standalone CLI to exercise the same DeviceTcpCommandClient
// the API uses, *and* a raw interactive Telnet mode for manual troubleshooting.
//
// Usage:
//   dotnet run -- power-on                 # test production SendPowerAsync(1)
//   dotnet run -- power-off                # test production SendPowerAsync(0)
//   dotnet run -- raw "pshow"              # login then send arbitrary command
//   dotnet run -- interactive              # open raw telnet session (type commands live)
//   dotnet run -- blast                    # fire-and-forget login+command in one burst
// ---------------------------------------------------------------------------

using System.Net.Sockets;
using System.Text;
using DefaultNamespace;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// ── Configuration ───────────────────────────────────────────────────────────
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var opts = new DeviceTcpOptions();
config.GetSection(DeviceTcpOptions.SectionName).Bind(opts);

// ── Logging ─────────────────────────────────────────────────────────────────
using var loggerFactory = LoggerFactory.Create(b =>
{
    b.AddConsole();
    b.SetMinimumLevel(LogLevel.Debug);
});
var logger = loggerFactory.CreateLogger<DeviceTcpCommandClient>();

// DebugConsoleOutput is already true in appsettings.json for this tool.

// ── Parse command ───────────────────────────────────────────────────────────
var verb = args.Length > 0 ? args[0].ToLowerInvariant() : "help";

switch (verb)
{
    case "power-on":
        await RunSendPowerAsync(1);
        break;

    case "power-off":
        await RunSendPowerAsync(0);
        break;

    case "raw":
        var rawCmd = args.Length > 1 ? string.Join(' ', args.Skip(1)) : "pshow";
        await RunRawCommandAsync(rawCmd);
        break;

    case "interactive":
        await RunInteractiveAsync();
        break;

    case "blast":
        var blastCmd = args.Length > 1 ? string.Join(' ', args.Skip(1)) : opts.PowerOnCommand;
        await RunBlastAsync(blastCmd);
        break;

    default:
        PrintHelp();
        break;
}

return;

// ═══════════════════════════════════════════════════════════════════════════
// Mode 1: Production path – exercises DeviceTcpCommandClient exactly as the API does
// ═══════════════════════════════════════════════════════════════════════════
async Task RunSendPowerAsync(int pwr)
{
    Banner($"SendPowerAsync({pwr}) – production code path");

    var wrapped = Options.Create(opts);
    var client = new DeviceTcpCommandClient(wrapped, logger);

    using var cts = new CancellationTokenSource(opts.TimeoutMs + 2000);
    var result = await client.SendPowerAsync(pwr, cts.Token);

    Console.WriteLine();
    Console.WriteLine("═══════════ RESULT ═══════════");
    Console.WriteLine($"  Success : {result.Success}");
    Console.WriteLine($"  Received: {result.ReceivedCommand}");
    Console.WriteLine($"  Device  : {result.DeviceCommand}");
    Console.WriteLine($"  Error   : {result.Error ?? "(none)"}");
    Console.WriteLine("══════════════════════════════");
}

// ═══════════════════════════════════════════════════════════════════════════
// Mode 2: Raw command – login then send any CLI command and print the response
// ═══════════════════════════════════════════════════════════════════════════
async Task RunRawCommandAsync(string command)
{
    Banner($"Raw command: {command}");

    using var tcp = new TcpClient();
    await tcp.ConnectAsync(opts.Host, opts.Port);
    Log("Connected");

    await using var stream = tcp.GetStream();

    // Read + negotiate banner
    var banner = await ReadAndNegotiate(stream);
    Log($"Banner: {Sanitize(banner)}");

    // Send credentials (no "login" command — device prompts User ID: directly)
    await SendLine(stream, opts.UserId);
    await Task.Delay(300);
    await SendLine(stream, opts.Password);
    await Task.Delay(500);

    // Drain any login response
    var loginResp = await DrainAvailable(stream);
    Log($"Login response: {Sanitize(loginResp)}");

    // Send the actual command
    await SendLine(stream, command);
    await Task.Delay(500);

    // Read back the command response
    var cmdResp = await DrainAvailable(stream);
    Console.WriteLine();
    Console.WriteLine("═══════════ DEVICE RESPONSE ═══════════");
    Console.WriteLine(cmdResp);
    Console.WriteLine("═══════════════════════════════════════");
}

// ═══════════════════════════════════════════════════════════════════════════
// Mode 3: Interactive raw telnet session
// ═══════════════════════════════════════════════════════════════════════════
async Task RunInteractiveAsync()
{
    Banner("Interactive Telnet session (type 'quit' to exit)");

    using var tcp = new TcpClient();
    await tcp.ConnectAsync(opts.Host, opts.Port);
    Log("Connected");

    await using var stream = tcp.GetStream();

    var banner = await ReadAndNegotiate(stream);
    Log($"Banner: {Sanitize(banner)}");

    // Start a background reader that prints everything from the device
    var readerCts = new CancellationTokenSource();
    var readerTask = Task.Run(async () =>
    {
        var buf = new byte[1024];
        try
        {
            while (!readerCts.Token.IsCancellationRequested)
            {
                var n = await stream.ReadAsync(buf, readerCts.Token);
                if (n == 0) break;
                var text = StripIac(buf.AsSpan(0, n));
                var hex = Convert.ToHexString(buf.AsSpan(0, n));
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(text);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"  [{hex}]");
                Console.ResetColor();
            }
        }
        catch (OperationCanceledException) { }
        catch (IOException) { }
    });

    // Read from stdin and send to device
    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("cmd> ");
        Console.ResetColor();

        var line = Console.ReadLine();
        if (line == null || line.Equals("quit", StringComparison.OrdinalIgnoreCase))
            break;

        await SendLine(stream, line);
        await Task.Delay(300); // give device time to respond before next prompt
    }

    readerCts.Cancel();
    try { await readerTask; } catch { /* swallow */ }
    Log("Session closed.");
}

// ═══════════════════════════════════════════════════════════════════════════
// Mode 4: Blast – send login+command in one burst (mirrors the serial script approach)
// ═══════════════════════════════════════════════════════════════════════════
async Task RunBlastAsync(string command)
{
    Banner($"Blast mode: login + {command}");

    using var tcp = new TcpClient();
    await tcp.ConnectAsync(opts.Host, opts.Port);
    Log("Connected");

    await using var stream = tcp.GetStream();

    // Read and negotiate banner
    var banner = await ReadAndNegotiate(stream);
    Log($"Banner: {Sanitize(banner)}");

    // Send credentials+command as a single burst (device prompts User ID: directly, no login command)
    var blast = $"{opts.UserId}\r\n{opts.Password}\r\n{command}\r\n";
    var payload = Encoding.ASCII.GetBytes(blast);
    Log($"TX blast ({payload.Length} bytes): <user> / <pass> / {command}");
    await stream.WriteAsync(payload);
    await stream.FlushAsync();

    // Wait and collect everything back
    await Task.Delay(1500);
    var response = await DrainAvailable(stream);

    Console.WriteLine();
    Console.WriteLine("═══════════ DEVICE RESPONSE ═══════════");
    Console.WriteLine(response);
    Console.WriteLine($"  (hex: {Convert.ToHexString(Encoding.ASCII.GetBytes(response))})");
    Console.WriteLine("═══════════════════════════════════════");
}

// ── Helpers ─────────────────────────────────────────────────────────────────

async Task<string> ReadAndNegotiate(NetworkStream stream)
{
    var buf = new byte[1024];
    stream.ReadTimeout = 2000;

    int n;
    try
    {
        n = await stream.ReadAsync(buf);
    }
    catch (IOException)
    {
        return string.Empty;
    }

    if (n == 0) return string.Empty;

    var rawHex = Convert.ToHexString(buf.AsSpan(0, n));
    Log($"RX initial ({n} bytes) hex={rawHex}");

    // Build IAC negotiation reply
    var reply = new List<byte>();
    var textBytes = new List<byte>();
    var i = 0;
    while (i < n)
    {
        if (buf[i] == 0xFF && i + 2 < n)
        {
            var cmd = buf[i + 1];
            var opt = buf[i + 2];
            if (cmd == 0xFB) // WILL → DONT
            {
                reply.AddRange(new byte[] { 0xFF, 0xFE, opt });
            }
            else if (cmd == 0xFD) // DO → WONT
            {
                reply.AddRange(new byte[] { 0xFF, 0xFC, opt });
            }
            i += 3;
        }
        else
        {
            textBytes.Add(buf[i]);
            i++;
        }
    }

    if (reply.Count > 0)
    {
        await stream.WriteAsync(reply.ToArray());
        Log($"TX negotiation ({reply.Count} bytes): {Convert.ToHexString(reply.ToArray())}");
    }

    return Encoding.ASCII.GetString(textBytes.ToArray());
}

async Task SendLine(NetworkStream stream, string line)
{
    var payload = Encoding.ASCII.GetBytes(line + "\r\n");
    Log($"TX ({payload.Length} bytes): {line}");
    await stream.WriteAsync(payload);
}

async Task<string> DrainAvailable(NetworkStream stream)
{
    var sb = new StringBuilder();
    var buf = new byte[1024];
    stream.ReadTimeout = 1000;

    try
    {
        while (true)
        {
            var n = await stream.ReadAsync(buf);
            if (n == 0) break;

            var text = StripIac(buf.AsSpan(0, n));
            var hex = Convert.ToHexString(buf.AsSpan(0, n));
            Log($"RX ({n} bytes) hex={hex} text='{Sanitize(text)}'");
            sb.Append(text);

            if (!stream.DataAvailable)
            {
                // Give it one more chance after a tiny delay
                await Task.Delay(200);
                if (!stream.DataAvailable) break;
            }
        }
    }
    catch (IOException) { /* read timeout – that's fine */ }

    return sb.ToString();
}

string StripIac(ReadOnlySpan<byte> data)
{
    var text = new List<byte>();
    var i = 0;
    while (i < data.Length)
    {
        if (data[i] == 0xFF && i + 2 < data.Length)
        {
            i += 3; // skip IAC sequences
        }
        else
        {
            text.Add(data[i]);
            i++;
        }
    }
    return Encoding.ASCII.GetString(text.ToArray());
}

string Sanitize(string s) => s
    .Replace("\r", "\\r")
    .Replace("\n", "\\n")
    .Replace("\0", "\\0");

void Log(string msg)
{
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write($"[{DateTime.Now:HH:mm:ss.fff}] ");
    Console.ResetColor();
    Console.WriteLine(msg);
}

void Banner(string title)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n  ▸ {title}");
    Console.WriteLine($"    Host: {opts.Host}:{opts.Port}  Timeout: {opts.TimeoutMs}ms  Login: {opts.UseLogin}");
    Console.ResetColor();
    Console.WriteLine();
}

void PrintHelp()
{
    Console.WriteLine("""
    DeviceTcpCliTester – Synaccess netBooter NP-0801DTG2 test tool

    Usage:
      dotnet run -- power-on              Test SendPowerAsync(1) via production client
      dotnet run -- power-off             Test SendPowerAsync(0) via production client
      dotnet run -- raw "pshow"           Login then send arbitrary CLI command
      dotnet run -- raw "pset 1 1"        Login then power on outlet 1
      dotnet run -- blast                 Fire login+command in a single TCP burst
      dotnet run -- blast "pset 1 1"      Blast with specific command
      dotnet run -- interactive           Open raw interactive telnet session
      dotnet run -- help                  Show this help

    Config: appsettings.json (same DeviceTcp section as the API)
    """);
}

