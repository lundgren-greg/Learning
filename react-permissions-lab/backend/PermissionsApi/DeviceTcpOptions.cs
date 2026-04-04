namespace DefaultNamespace;

public sealed class DeviceTcpOptions
{
    public const string SectionName = "DeviceTcp";

    public string Host { get; init; } = "127.0.0.1";
    public int Port { get; init; } = 9000;
    public int TimeoutMs { get; init; } = 5000;
    public string PowerOnCommand { get; init; } = "pset 1 1";
    public string PowerOffCommand { get; init; } = "pset 1 0";
    public bool UseLogin { get; init; } = false;
    public string LoginCommand { get; init; } = "login";
    public string UserId { get; init; } = "";
    public string Password { get; init; } = "";
}
