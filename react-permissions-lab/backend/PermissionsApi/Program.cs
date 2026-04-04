using DefaultNamespace;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Allow the React dev server (Vite default port) to call this API
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDevClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.Configure<DeviceTcpOptions>(
    builder.Configuration.GetSection(DeviceTcpOptions.SectionName));
builder.Services.AddSingleton<IDeviceTcpCommandClient, DeviceTcpCommandClient>();

var app = builder.Build();

app.UseCors("ReactDevClient");

// GET /api/permissions?role=user|admin
// Returns the permissions associated with the given role.
// In a real application this would look up the authenticated user's role from
// a database or identity provider.  Here it simply maps the role query
// parameter to a set of permissions so you can explore the API shape.
app.MapGet("/api/permissions", (string role) =>
{
    var normalised = role.Trim().ToLowerInvariant();

    var result = normalised switch
    {
        "admin" => new PermissionsResponse(Role: "admin", HasAdminAccess: true),
        "greg" => new PermissionsResponse(Role: "greg", HasAdminAccess: true),
        _       => new PermissionsResponse(Role: "user",  HasAdminAccess: false),
    };

    return Results.Ok(result);
})
.WithName("GetPermissions");

app.MapPost("/api/device/power", async (
    DevicePowerRequest request,
    IDeviceTcpCommandClient tcpCommandClient,
    CancellationToken cancellationToken) =>
{
    // TODO(power-off): Expand validation to allow 0 once off-flow wiring is complete.
    if (request.Pwr != 1)
    {
        return Results.BadRequest(new { error = "pwr=1 (Power On) is currently supported." });
    }

    // TODO(power-off): Extend SendPowerAsync mapping/tests for pwr=0 once frontend off button is implemented.
    var result = await tcpCommandClient.SendPowerAsync(request.Pwr, cancellationToken);
    if (!result.Success)
    {
        return Results.Problem(
            title: "Failed to send TCP command to device",
            detail: result.Error,
            statusCode: StatusCodes.Status502BadGateway);
    }

    return Results.Ok(new DevicePowerResponse(
        Success: true,
        ReceivedCommand: result.ReceivedCommand,
        DeviceCommand: result.DeviceCommand));
})
.WithName("SendDevicePowerCommand");

app.Run();

record PermissionsResponse(string Role, bool HasAdminAccess);
record DevicePowerRequest(int Pwr);
record DevicePowerResponse(bool Success, string ReceivedCommand, string DeviceCommand);

public partial class Program;
