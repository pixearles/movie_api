# Movies API

.NET 10 solution, MVC-style controllers (`[ApiController]`), building up a read API first with a front end to follow later.

## Solution layout

Three projects:

- **Movies.API** — the web API. Controllers, vertical-slice features, and tests all live here.
- **Movies.Domain** — every entity in the solution. No dependencies on the other projects.
- **Movies.Infrastructure** — `MoviesDbContext`, EF Core migrations, entity `Configuration/`, and `Seeding/`. Depends on `Movies.Domain`. Exposes `AddInfrastructure(IServiceCollection, connectionString)` so `Movies.API` never references EF Core packages directly.

Data access is **EF Core** against SQL Server. Local dev database is `MoviesDb` on the local `SQLEXPRESS` instance, Windows/trusted auth (connection string in `Movies.API/appsettings.json`).

Create new migrations from the repo root:
```
dotnet ef migrations add <Name> --project Movies.Infrastructure --startup-project Movies.API
```

To bring the database schema and seed data up to date, use the app itself rather than raw `dotnet ef database update` — the EF CLI can only apply migrations, it can't run our seeding code:
```
dotnet run --project Movies.API -- migrate
```
This applies any pending migrations, then seeds — but only the very first time the database is created (checked via whether any migration has been applied yet, *not* whether `Movies` is empty, so manually clearing seeded data later won't trigger a reseed) — then exits without starting the web host. A plain `dotnet run --project Movies.API` does the same migrate+seed step automatically before it starts listening, so this command is only needed when you want the database updated without launching the API.

## Seeding

`Movies.Infrastructure/Seeding/` seeds `Movies`, `Genres`, and `MovieGenres` from `Movies.Infrastructure/OriginalData/mymoviedb.csv` (~9,827 rows) via CsvHelper. One file per entity (`GenreSeeder.cs`, `MovieSeeder.cs`, `MovieGenreSeeder.cs`), orchestrated by `DataSeeder.cs`: genres are seeded first (deduped from each row's comma-separated `Genre` column), then movies, then `MovieGenres` joins built by re-splitting each row's genre list against the now-persisted movies/genres. Plain static classes, no DI — same style as `DependencyInjection.AddInfrastructure`.

`Actors` have no source data in the CSV, so `ActorSeeder.cs` generates 500 names instead, combining small first/last name pools with a fixed-seed `Random` (reproducible across drop/recreate cycles, not `Random.Shared`). `MovieActorSeeder.cs` then assigns each movie a random 3–6 person cast from that pool (same fixed-seed approach), and guarantees every actor ends up credited in at least one movie.

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
