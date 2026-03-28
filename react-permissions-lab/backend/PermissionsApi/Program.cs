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

app.Run();

record PermissionsResponse(string Role, bool HasAdminAccess);

public partial class Program;

