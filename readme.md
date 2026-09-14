# Movies API

A read-focused movie search API built for the Optix technical test, using the [9000 Movies Dataset](https://www.kaggle.com/datasets/disham993/9000-movies-dataset) as source data.

## Tech stack

- **.NET 10 / ASP.NET Core Web API**, using conventional `[ApiController]` controllers rather than Minimal APIs — a more familiar shape for a production service, one I work with daily.
- **Vertical slice architecture**, no MediatR. Each feature lives in its own folder (`Features/{Feature}/{SubFeature}/v1/`) with its own request/response models, handler, repository, and validator, wired together via a shared partial class per subfeature. A separate `{SubFeature}Controller` (non-partial) references into that shared class. This avoids the shared/anaemic layers a more traditional layered architecture tends to accumulate, and keeps each feature fully self-contained, no repository or handler is shared across slices.
- **EF Core against SQL Server**, chosen over PostgreSQL and SQLite mostly due to experience with SQLServer.
- **FluentValidation** for request validation, called explicitly from the controller (via a small shared `ApiControllerBase` helper) rather than inside the handler, so a bad request never reaches business logic and handlers can assume valid input.
- **No authentication.** Every endpoint is public and read-only, and the brief's requirements never call for a user concept. Deliberately left out rather than built speculatively — see "Design decisions" below.
- **Blazor WebAssembly + MudBlazor** (`Movies.Web` / `Movies.Web.Client`) for the front end — see "Front end" below.
- **Docker + Docker Compose** for deployment — a `db` service (SQL Server, Linux container), an `api` service, and a `web` service (the front end), with healthchecks/dependencies ensuring each waits for what it needs before starting.

## Running the project

### Locally (no Docker)

Requires the .NET 10 SDK and a local SQL Server instance (developed against SQL Server Express, `MoviesDb` database, Windows/trusted auth — see `Movies.API/appsettings.json` for the connection string).

```
dotnet run --project Movies.API
```

On first run, this automatically applies EF Core migrations and seeds the database ~9,800 movies, genres, and actors, (actors were not present in the original dataset so I created a randomised set for use with the api), from `Movies.Infrastructure/OriginalData/mymoviedb.csv`. Subsequent runs skip seeding, since it's guarded by whether any migration has already been applied — not by whether the tables are empty — so manually clearing seeded data won't accidentally trigger a reseed.

To apply migrations and seed without starting the web server:
```
dotnet run --project Movies.API -- migrate
```

With the API running, the front end is a separate process:
```
dotnet run --project Movies.Web
```
This starts the Blazor Web App host, which serves the WebAssembly client; the client calls `Movies.API` directly from the browser (see "Front end" below), so the API needs to already be running. Watch the console output for the actual URL (`https://localhost:7135` by default) and open it in a browser.

### With Docker

```
docker compose up --build
```

This builds the API and front-end images, starts a SQL Server container, waits for it to report healthy, then starts the API and the front end — the API runs the same migrate-and-seed step as above against the fresh containerised database. First run will take a few minutes (image downloads plus a full seed of ~9,800 rows); subsequent runs are faster.

Once running, the API is available at `http://localhost:8080` and the front end at `http://localhost:8081`.

**Note:** the SQL Server container's `SA_PASSWORD` is hardcoded in `docker-compose.yml` for convenience in this exercise. A real deployment would pull this from a secrets manager or environment-specific configuration rather than committing it to source control, in my current position we use AzureDevOps KeyVault.

## Endpoints

### `GET /api/movies/search/v1`

Search, filter, sort, and page the movie catalogue. All parameters are optional query-string values.

| Parameter | Type | Notes |
|---|---|---|
| `searchTerm` | string | Matches against movie title. |
| `genres` | int[] | Genre IDs. A movie matches if it has **any** of the listed genres (OR) — genres are overlapping categorical tags, so this reads naturally as "show me anything in either bucket." |
| `actors` | int[] | Actor IDs. A movie matches only if it has **all** of the listed actors (AND) — picking specific named people is a co-occurrence condition ("find films with both X and Y"), not a category selection, so this deliberately behaves differently from `genres`. Duplicate IDs are rejected by validation. |
| `sortBy` | string | `title` or `releaseDate` (case-insensitive). Defaults to title. |
| `sortByDescending` | bool | Defaults to `false`. |
| `pageNumber` | int | Defaults to `1`. |
| `pageSize` | int | Defaults to `20`, capped at `100`. |

Response includes the page of results plus `totalCount`, `pageNumber`, `pageSize`, and `totalPages` for client-side pagination controls.

### `GET /api/actors/search/v1`

Search and page actors by name, generated by the `/add-slice` Claude Code skill (see below) as a close structural match to the movie search endpoint.

| Parameter | Type | Notes |
|---|---|---|
| `searchTerm` | string | Matches against actor name. |
| `sortByDescending` | bool | Defaults to `false`. Only one sortable column (`Name`), so there's no separate `sortBy`. |
| `pageNumber` | int | Defaults to `1`. |
| `pageSize` | int | Defaults to `20`, capped at `100`. |

Response includes the page of results plus `totalCount`, `pageNumber`, `pageSize`, and `totalPages`. Each actor entry has `id` and `name`.

### `GET /api/movies/get-movie-details/v1/{id}`

Fetch full details for a single movie by ID.

| Parameter | Type | Notes |
|---|---|---|
| `id` | int (route) | The movie ID. |

Response includes title, release date, overview, poster URL, average vote, popularity, and original language, plus nested `genres` (`id`, `name`) and `actors` (`id`, `name`) lists. Returns `404` if no movie matches the ID.

### `GET /api/genres/get-all/v1`

List every genre. No paging or filtering.

Response is a flat array of `{ id, name }`, ordered by name.

### `GET /api/genres/search/v1`

Search and page genres by name — structurally identical to `SearchActors`.

| Parameter | Type | Notes |
|---|---|---|
| `searchTerm` | string | Matches against genre name. |
| `sortByDescending` | bool | Defaults to `false`. Only one sortable column (`Name`), so there's no separate `sortBy`. |
| `pageNumber` | int | Defaults to `1`. |
| `pageSize` | int | Defaults to `20`, capped at `100`. |

Response includes the page of results plus `totalCount`, `pageNumber`, `pageSize`, and `totalPages`. Each genre entry has `id` and `name`.

## Front end

A minimal Blazor front end sits alongside the API, split into two projects using .NET's standard Blazor Web App template:

- **`Movies.Web`** — the ASP.NET Core host. Serves the WebAssembly bundle and static assets; carries no business logic of its own.
- **`Movies.Web.Client`** — the Blazor WebAssembly project containing all the interactive UI. Runs entirely in the browser and calls `Movies.API`'s endpoints directly over HTTP (CORS is enabled on the API for this project's origin) rather than routing requests through the host as a backend-for-frontend.

**MudBlazor** was chosen as the component library. The main reason for this was my familiarity with it from my day to day work — but it holds up well on its own merits for a project like this:
- The components available provide a very nice UX
- Not needing js, and keeping everything in C# keeps the code standardised and easier to read and manage across the whole solution.
- I've been using it for several years now, and found it's improved the quality, and ease of development of front ends.

Like the API, the client is organised as vertical slices, under `Movies.Web.Client/Features/{Feature}/{SubFeature}/`, mirroring the API's own feature names and `Request`/`Response` shapes field-for-field — deliberately *without* a `v1` segment, since API versioning is the server's concern, not the client's; a future `v2` only means changing a URL string inside that slice's `ApiClient.cs`, not restructuring folders.

## Design decisions

A few choices worth explaining rather than leaving implicit:

- **No generic repository.** Each slice has its own small repository rather than a shared `IRepository<T>` base — EF Core's `DbSet<T>` already provides GetAll/GetById equivalents, and a shared base class would reintroduce the cross-slice coupling vertical slices are meant to avoid, for a saving of only a few boilerplate lines given the small number of entities here.
- **`Genre` and `Actor` are many-to-many with `Movie` via explicit join entities** (`MovieGenre`, `MovieActor`), not EF Core's implicit many-to-many — chosen so the join tables are visible, queryable, first-class entities rather than EF-managed and invisible.
- **No authorization.** Every current endpoint is public and read-only, so there's nothing to protect. If write operations or sensitive data are added later, the vertical-slice pattern already has a slot for this (a nested `IAuthorizer`/`Authorizer` per slice), deliberately left out for now rather than built speculatively.

## AI-assisted development

In line with the disciplined, transparent use of AI tools this role is looking for:

- The `SearchMovies` slice (`Features/Movies/Search/v1/`) was hand-written without the use of AI
- Once that pattern was proven out and stable, a Claude Code skill (`/add-slice`) was created to scaffold new slices following the same structure — nested partial class, `Request`/`Response`/`Validator`/`Handler`/`Repository`, separate controller, FluentValidation wiring. This was used to generate the `SearchActors` and `SearchGenres` slices, which closely mirror `SearchMovies` in shape.
- Output from the skill was reviewed before being committed, not accepted blindly — the point of building the skill after establishing the pattern by hand, rather than generating everything from the start, was to make sure the pattern itself was sound and well-understood before automating its repetition. That review caught the skill conflating a slice's folder name with its partial class name (producing an incorrectly-named `SearchActors`/`GetAllGenres`), which was fixed by adding an explicit, separate step to the skill for confirming the class name.
- A second skill (`/add-tests`) was created to generate MSTest + Moq tests for a slice once its `Handler.cs`/`Validator.cs` are actually implemented, deriving test cases from the real logic rather than templating blindly.

## Further development

If I continued to develop the app there are several things I would like to add and extend:

- Role name against the actor for a movie
- Headshots for actors
- Adding the ability to display crew details also
- Additional information against actors