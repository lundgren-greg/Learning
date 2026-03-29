// TODO: Add .NET Aspire to this project to provide a hosted database (e.g. PostgreSQL
//       or SQL Server) via the Aspire AppHost.  Steps:
//         1. Create an Aspire AppHost project at the solution root.
//         2. Reference PermissionsApi from the AppHost and wire up a database resource.
//         3. Inject the Aspire-provided connection string into this service.

// TODO: Define the database table for tracking role selections.  Suggested schema:
//
//   Table: RoleSelections
//   ┌─────────────┬──────────────────────┬──────────────────────────────────┐
//   │ Column      │ Type                 │ Notes                            │
//   ├─────────────┼──────────────────────┼──────────────────────────────────┤
//   │ Id          │ INT IDENTITY PK      │ Auto-increment primary key       │
//   │ Role        │ NVARCHAR(50) NOT NULL│ The role that was requested      │
//   │ SelectedAt  │ DATETIMEOFFSET       │ UTC timestamp of the selection   │
//   │ UserId      │ NVARCHAR(100) NULL   │ NULL until multi-user is added   │
//   └─────────────┴──────────────────────┴──────────────────────────────────┘
//
// TODO (multi-user): Add a Users table and a foreign key from RoleSelections.UserId
//       to Users.Id once multiple-user support is implemented.  This will require a
//       database migration (e.g. via EF Core "dotnet ef migrations add AddUsersTable").

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

// TODO: Register a DbContext (e.g. PermissionsDbContext) here once the Aspire DB
//       resource and EF Core packages have been added to the project.
//       Example:
//         builder.Services.AddDbContext<PermissionsDbContext>(options =>
//             options.UseSqlServer(builder.Configuration.GetConnectionString("PermissionsDb")));

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

    // TODO: Persist the selection to the database so the last-chosen role can be
    //       retrieved later.  Example using a hypothetical DbContext:
    //         db.RoleSelections.Add(new RoleSelection { Role = result.Role, SelectedAt = DateTimeOffset.UtcNow });
    //         await db.SaveChangesAsync();
    //
    // TODO (multi-user): Once multi-user support is added, also store the authenticated
    //       user's identifier (e.g. from HttpContext.User) alongside the selection.
    //       Implementing this will require a database migration to add the Users table
    //       and the foreign key described above.

    return Results.Ok(result);
})
.WithName("GetPermissions");

// TODO: Add a GET /api/permissions/last endpoint that returns the most recently
//       persisted RoleSelection row from the database, so the frontend can restore
//       the last selection on page load.

app.Run();

record PermissionsResponse(string Role, bool HasAdminAccess);

public partial class Program;

