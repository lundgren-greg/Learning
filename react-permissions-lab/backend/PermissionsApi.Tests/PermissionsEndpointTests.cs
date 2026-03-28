namespace PermissionsApi.Tests;

public sealed class PermissionsEndpointTests : IClassFixture<PermissionsApiWebAppFactory>
{
    private readonly HttpClient _client;

    public PermissionsEndpointTests(PermissionsApiWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("admin", "admin", true)]
    [InlineData("greg", "greg", true)]
    [InlineData("user", "user", false)]
    [InlineData("something-else", "user", false)]
    public async Task GetPermissions_ReturnsExpectedRoleMapping(string requestedRole, string expectedRole, bool expectedAdminAccess)
    {
        var response = await _client.GetAsync($"/api/permissions?role={Uri.EscapeDataString(requestedRole)}");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PermissionsResponseDto>();

        Assert.NotNull(payload);
        Assert.Equal(expectedRole, payload!.Role);
        Assert.Equal(expectedAdminAccess, payload.HasAdminAccess);
    }

    [Fact]
    public async Task GetPermissions_TrimAndCaseNormalizesRole()
    {
        var response = await _client.GetAsync("/api/permissions?role=%20AdMiN%20");

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PermissionsResponseDto>();

        Assert.NotNull(payload);
        Assert.Equal("admin", payload!.Role);
        Assert.True(payload.HasAdminAccess);
    }

    [Fact]
    public async Task GetPermissions_WithoutRoleQuery_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/permissions");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record PermissionsResponseDto(string Role, bool HasAdminAccess);
}

