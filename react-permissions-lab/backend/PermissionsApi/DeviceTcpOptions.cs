namespace DefaultNamespace;

public sealed class DeviceTcpOptions
{
    public const string SectionName = "DeviceTcp";

    public string Host { get; init; } = "127.0.0.1";
    public int Port { get; init; } = 9000;
    public int TimeoutMs { get; init; } = 5000;
}
