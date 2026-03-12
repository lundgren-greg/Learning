# #asp-contract-lab TODOs

This file contains the ordered exercise steps, with explicit edits and example snippets to copy into the projects. Follow each step, run the quick commands, and mark the checkbox when done.

Prerequisites
- .NET SDK 8 (or matching the project target) installed and on PATH.
- From repository root (`C:/Repos/Learning`).

Step 1 — Add contract DTOs and interfaces
Files to edit: `BookStore.Contracts/Class1.cs` (or create new files).

Create the following files (suggested names):
- `Contracts/Models/BookDto.cs`
- `Contracts/Requests/CreateBookRequest.cs`
- `Contracts/Services/IBookService.cs`

Example snippets

BookDto.cs
```csharp
namespace BookStore.Contracts.Models
{
    public record BookDto(Guid Id, string Title, string Author, int Year);
}
```

CreateBookRequest.cs
```csharp
namespace BookStore.Contracts.Requests
{
    public class CreateBookRequest
    {
        public required string Title { get; set; }
        public required string Author { get; set; }
        public int Year { get; set; }
    }
}
```

IBookService.cs
```csharp
using BookStore.Contracts.Models;
using BookStore.Contracts.Requests;

namespace BookStore.Contracts.Services
{
    public interface IBookService
    {
        Task<BookDto> CreateAsync(CreateBookRequest request);
        Task<BookDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<BookDto>> ListAsync();
    }
}
```

Step 2 — Reference Contracts from API
- Ensure `BookStore.Api/BookStore.Api.csproj` contains a ProjectReference to `../BookStore.Contracts/BookStore.Contracts.csproj`.

Step 3 — Implement controller in `BookStore.Api`
- Recommended path: `BookStore.Api/Controllers/BooksController.cs`.

Example controller
```csharp
using BookStore.Contracts.Models;
using BookStore.Contracts.Requests;
using BookStore.Contracts.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _service;

    public BooksController(IBookService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookRequest req)
    {
        var created = await _service.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var book = await _service.GetByIdAsync(id);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _service.ListAsync());
}
```

Step 4 — Add an in-memory implementation
- Location: `BookStore.Api/Services/InMemoryBookService.cs`

Snippet
```csharp
using BookStore.Contracts.Models;
using BookStore.Contracts.Requests;
using BookStore.Contracts.Services;

public class InMemoryBookService : IBookService
{
    private readonly List<BookDto> _store = new();

    public Task<BookDto> CreateAsync(CreateBookRequest request)
    {
        var book = new BookDto(Guid.NewGuid(), request.Title, request.Author, request.Year);
        _store.Add(book);
        return Task.FromResult(book);
    }

    public Task<BookDto?> GetByIdAsync(Guid id) => Task.FromResult(_store.FirstOrDefault(b=>b.Id==id));

    public Task<IEnumerable<BookDto>> ListAsync() => Task.FromResult<IEnumerable<BookDto>>(_store.ToList());
}
```

Register the service in `Program.cs` (or `Startup`) in `BookStore.Api`
```csharp
builder.Services.AddSingleton<IBookService, InMemoryBookService>();
```

Step 5 — Tests
- Add a basic unit test in `BookStore.Tests` to validate the `InMemoryBookService` or controller.

Example xUnit test
```csharp
using BookStore.Contracts.Requests;
using BookStore.Api.Services; // or the namespace you used

public class BookServiceTests
{
    [Fact]
    public async Task CreateAndGet_ReturnsSame()
    {
        var svc = new InMemoryBookService();
        var req = new CreateBookRequest { Title = "T", Author = "A", Year = 2020 };
        var created = await svc.CreateAsync(req);
        var fetched = await svc.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
    }
}
```

Step 6 — Build and run tests
- From repo root run:
```powershell
dotnet build AspContractLab.sln
dotnet test AspContractLab.sln
```

Optional next steps (stretch goals)
- Add Swagger/OpenAPI to `BookStore.Api` and validate contract shapes.
- Add model validation and error responses.
- Add integration tests using `WebApplicationFactory<TEntryPoint>` to exercise controllers over HTTP.

If you'd like I can apply these changes directly into the projects (small, incremental commits). Say "apply starter code" and I'll make the edits, run build/tests, and report results.
