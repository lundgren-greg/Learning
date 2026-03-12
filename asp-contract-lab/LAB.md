# #asp-contract-lab — API Contract Implementation Lab

Goal: teach you to design a contract (shared DTOs + interfaces), wire it into an ASP.NET Web API project, implement the API, and verify it with tests.

Duration: 60–120 minutes (depends on familiarity).

High-level plan
- Define a minimal, versionable Contracts project with DTOs and service interfaces.
- Reference the Contracts project from `BookStore.Api` and implement controllers that expose the contract.
- Add tests in `BookStore.Tests` (unit + minimal integration) that validate the contract surface and runtime behavior.
- Run the build, run tests, and do a small smoke run of the API.

Checklist (follow as you work)
1. [ ] Create/expand `BookStore.Contracts` DTOs and interfaces (models, requests, responses).
2. [ ] Add or verify project reference from `BookStore.Api` -> `BookStore.Contracts`.
3. [ ] Implement a `BooksController` in `BookStore.Api` using the contract types.
4. [ ] Add a simple in-memory service that implements the contract interface.
5. [ ] Add tests in `BookStore.Tests` that reference `BookStore.Contracts` and `BookStore.Api` as needed.
6. [ ] Build the solution and run tests.

Contract summary
- Inputs: HTTP requests mapped to contract request models (e.g., `CreateBookRequest`), path/query params.
- Outputs: Clear response shapes (e.g., `BookDto`, `PagedResult<BookDto>`), HTTP status codes (201 for create, 200 for success, 404 for not found).
- Error modes: validation errors (400), not found (404), unexpected server error (500).

Edge cases to consider
- Missing/invalid model fields (null/empty strings). Use model validation attributes.
- Duplicate resource creation.
- Pagination boundaries.

Quality gates
- Build: `dotnet build AspContractLab.sln` should succeed.
- Tests: `dotnet test AspContractLab.sln` should run unit tests.

Quick commands (PowerShell)
```powershell
# from repository root
dotnet build AspContractLab.sln
dotnet test AspContractLab.sln

# run the API (from repo root)
dotnet run --project asp-contract-lab/BookStore.Api/BookStore.Api.csproj
```

Notes and next steps
- See `TODOs.md` for concrete, ordered edits and snippets to copy into the projects. Use it as your guided exercise sheet and check off tasks as you go.
