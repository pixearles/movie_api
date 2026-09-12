# Movies API

.NET 10 solution, MVC-style controllers (`[ApiController]`), building up a read API first with a front end to follow later.

## Solution layout

Three projects:

- **Movies.API** — the web API. Controllers, vertical-slice features, and tests all live here.
- **Movies.Domain** — every entity in the solution. No dependencies on the other projects.
- **Movies.Infrastructure** — `MoviesDbContext`, EF Core migrations, entity `Configuration/`, and `Seeding/`. Depends on `Movies.Domain`. Exposes `AddInfrastructure(IServiceCollection, connectionString)` so `Movies.API` never references EF Core packages directly.

Data access is **EF Core** against SQL Server. Local dev database is `MoviesDb` on the local `SQLEXPRESS` instance, Windows/trusted auth (connection string in `Movies.API/appsettings.json`).

Run migrations from the repo root:
```
dotnet ef migrations add <Name> --project Movies.Infrastructure --startup-project Movies.API
dotnet ef database update --project Movies.Infrastructure --startup-project Movies.API
```

## Vertical slice structure

Inside `Movies.API/Features/`:

```
Features/
  {Feature}/
    {SubFeature}/
      v1/
        Models/           Request(s) and Response(s)
        Controller.cs
        Handler.cs
        Repository.cs     (when the slice needs its own data access)
        Validator.cs       (when the slice needs input validation)
        Tests.cs
```

Version folders are always `v1` for this app (no versioning strategy beyond that yet).

**Pattern**: each `.cs` file in a slice's `v1/` folder (except `Models/`) is a **partial declaration of one shared class**, named after the sub-feature and ending in `Controller` (e.g. `GetMovieController`) — ASP.NET Core's default controller discovery requires the class name to end in `Controller` or carry `[Controller]`. Each file contributes a nested interface + implementation pair for its concern:

- `Handler.cs` → nested `IHandler` / `Handler`
- `Repository.cs` → nested `IRepository` / `Repository`
- `Validator.cs` → nested `IValidator` / `Validator`
- `Controller.cs` → the `[ApiController]`/`[Route]` declaration, DI constructor, and action methods

Example shape:
```csharp
// Controller.cs
[ApiController]
[Route("api/movies/{id:int}")]
public partial class GetMovieController : ControllerBase
{
    private readonly IHandler _handler;

    public GetMovieController(IHandler handler) => _handler = handler;

    [HttpGet]
    public async Task<ActionResult<GetMovieResponse>> Get(int id) => await _handler.HandleAsync(id);
}

// Handler.cs
public partial class GetMovieController
{
    public interface IHandler
    {
        Task<GetMovieResponse> HandleAsync(int id);
    }

    public class Handler(IRepository repository) : IHandler
    {
        public async Task<GetMovieResponse> HandleAsync(int id) => await repository.GetAsync(id);
    }
}
```

No mediator library (no MediatR) — controllers call their handler directly via constructor-injected DI.

**No Authorizer.cs for now.** Every endpoint is public and read-only until that changes; the authorizer piece of the pattern is dropped for the time being. Re-introduce it (nested `IAuthorizer`/`Authorizer`, checked by the handler) once writes or auth requirements show up.

**Tests live in-place**: `Tests.cs` sits in the same `v1/` folder as the other files, in the `Movies.API` project itself (not a separate test project). Framework is **MSTest** (`[TestClass]`/`[TestMethod]`) with **Moq** for mocking the nested interfaces (`IRepository`, etc.).

## Coding standards

- Private fields: `_camelCase`.
- Local variables and parameters: `camelCase`.
- Use `var` when declaring variables.
- One-line methods/properties use expression-bodied syntax: `public string MyString() => result;`
- Single-line `if` statements omit braces:
  ```csharp
  if (true)
      return;
  ```
