---
name: add-slice
description: Scaffold a new vertical-slice feature (Controller/Handler/Repository/Validator/Models) for the Movies API, following the pattern documented in CLAUDE.md and the existing SearchMovies slice. Use when the user wants to add a new API endpoint/feature/sub-feature to Movies.API.
---

# Add a vertical slice

Scaffolds a new vertical-slice sub-feature under `Movies.API/Features/`, matching the pattern already used by `Movies.API/Features/Movies/Search/v1/` and documented in the repo's `CLAUDE.md`. This skill is always interactive — it never accepts inline arguments. Do not shortcut the question phase even if the user's invocation message already contains some of the answers; confirm each item explicitly through the questions below (pre-filling a question with a value they already gave you is fine, asking it silently is not).

Do not generate a `Tests.cs` file. Unit tests for new slices are handled by a separate skill.

## Step 1 — Ask questions, in this order

Use `AskUserQuestion` for anything with a short, enumerable set of answers. Use plain conversation for open-ended items (field lists). Ask one topic at a time; don't bundle the field-list questions into the same message as the pick-list questions.

1. **Feature name.** Look at the current contents of `Movies.API/Features/` (each top-level folder is a feature, e.g. `Movies`). Offer the existing feature name(s) found plus an "other / new feature" option. This is PascalCase and becomes the `{Feature}` folder segment.
2. **Sub-feature name.** Free text, PascalCase (e.g. `GetMovie`, `CreateMovie`, `DeleteMovie`). This becomes the `{SubFeature}` folder name, the grouping class name, and (with `Controller` appended) the controller class name. If what's given isn't PascalCase, ask again rather than auto-converting it — don't guess at word boundaries.
3. **HTTP method.** One of `GET` / `POST` / `PUT` / `PATCH` / `DELETE`.
4. **Route.** Free text (e.g. `api/movies/{id:int}` or `api/movies/create/v1`). There is no fixed convention in this codebase yet (CLAUDE.md's own example and the real `SearchMovies` route don't agree), so always ask — never assume `api/{feature}/{subfeature}/v1` or any other shape.
5. **Does this slice need its own `Repository.cs`?** (data access via `MoviesDbContext`). Yes/no.
6. **Does this slice need its own `Validator.cs`?** (FluentValidation input validation). Yes/no.
7. **Request shape.** Ask conversationally for the field list: name + type for each. For `GET`/`DELETE`, ask which fields come from the route (`[FromRoute]`) vs. the query string (`[FromQuery]`). For `POST`/`PUT`/`PATCH`, the whole `Request` is bound `[FromBody]`.
8. **Response shape.** Ask conversationally for the field list: name + type for each, including any nested supporting types the response needs (mirroring how `SearchMovies.Response` nests a separate `MovieSummary` type in the same Models file — see reference below).

## Step 2 — Confirm before writing

Summarize back: the full folder path, every file that will be created, whether Repository/Validator are included, the route, and the exact `Program.cs` lines that will be added. Get explicit go-ahead before creating anything.

## Step 3 — Guard rails

- If `Movies.API/Features/{Feature}/{SubFeature}/v1/` already exists, stop and tell the user — do not overwrite.
- If the feature or sub-feature name isn't valid PascalCase, go back and ask again.

## Step 4 — Generate files

Create `Movies.API/Features/{Feature}/{SubFeature}/v1/` containing the files below. Every file (other than `Controller.cs`) declares `public partial class {SubFeature}` in namespace `Movies.API.Features.{Feature}.{SubFeature}.v1`. Follow the coding standards from `CLAUDE.md`: `_camelCase` private fields, `camelCase` locals/params, `var` for local variables, expression-bodied one-liners, braceless single-line `if`.

### `Models/Request.cs`

```csharp
namespace Movies.API.Features.{Feature}.{SubFeature}.v1
{
    public partial class {SubFeature}
    {
        public class Request
        {
            // one property per requested field, e.g.:
            public int Id {get;set;}
        }
    }
}
```

### `Models/Response.cs`

```csharp
namespace Movies.API.Features.{Feature}.{SubFeature}.v1
{
    public partial class {SubFeature}
    {
        public class Response
        {
            // one property per requested field
        }

        // any additional nested supporting types the response needs, named plainly
        // (e.g. MovieSummary in SearchMovies.Response) go here in the same file
    }
}
```

### `Repository.cs` (only if requested)

```csharp
using Microsoft.EntityFrameworkCore;
using Movies.Infrastructure.Data;

namespace Movies.API.Features.{Feature}.{SubFeature}.v1
{
    public partial class {SubFeature}
    {
        public interface IRepository
        {
            Task<TODO> {SubFeature}Async(Request request);
        }

        public class Repository(MoviesDbContext context) : IRepository
        {
            public async Task<TODO> {SubFeature}Async(Request request)
            {
                // TODO: EF Core query
            }
        }
    }
}
```

Replace `TODO` with a real return shape once you know what the handler needs back (often the relevant domain entity or entities from `Movies.Domain.Entities`); leave the query body as a `// TODO` comment rather than guessing at business logic.

### `Validator.cs` (only if requested)

```csharp
using FluentValidation;

namespace Movies.API.Features.{Feature}.{SubFeature}.v1
{
    public partial class {SubFeature}
    {
        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                // TODO: RuleFor(...) per field
            }
        }
    }
}
```

### `Handler.cs`

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Movies.API.Features.{Feature}.{SubFeature}.v1
{
    public partial class {SubFeature}
    {
        public interface IHandler
        {
            Task<ActionResult<Response>> {SubFeature}Async(Request request);
        }

        public class Handler(IRepository repository) : IHandler
        {
            public async Task<ActionResult<Response>> {SubFeature}Async(Request request)
            {
                // TODO: call repository, map to Response
            }
        }
    }
}
```

If no `Repository.cs` was requested, drop the `IRepository repository` constructor parameter and leave a `// TODO` for what the handler actually does instead.

### `Controller.cs`

Not part of the grouping class's partial declaration — a separate, non-partial class.

With a validator:

```csharp
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Common;

namespace Movies.API.Features.{Feature}.{SubFeature}.v1
{
    [ApiController]
    [Route("{route}")]
    public class {SubFeature}Controller({SubFeature}.IHandler handler, IValidator<{SubFeature}.Request> validator)
        : ApiControllerBase
    {
        [Http{Method}]
        public async Task<ActionResult<{SubFeature}.Response>> {SubFeature}Async({BindingAttribute}{SubFeature}.Request request)
        {
            var validationError = await ValidateAsync(validator, request);

            if (validationError is not null)
                return validationError;

            return await handler.{SubFeature}Async(request);
        }
    }
}
```

Without a validator, drop the `IValidator<...> validator` parameter and the `ValidateAsync` call — just `return await handler.{SubFeature}Async(request);`.

`{BindingAttribute}` is `[FromQuery]` for a `GET`/`DELETE` request bound as a whole object (matching `SearchMoviesController`), or omitted when individual route parameters are used instead (e.g. `int id` bound via route template placeholders) — match whatever binding the user described in Step 1's request-shape question.

## Step 5 — Update `Movies.API/Program.cs`

1. Add `using Movies.API.Features.{Feature}.{SubFeature}.v1;` to the top, grouped with the other `Movies.API.Features...` using statements.
2. Under the `// feature slice registration` comment, add (in this order, only the lines that apply):
   ```csharp
   builder.Services.AddScoped<{SubFeature}.IHandler, {SubFeature}.Handler>();
   builder.Services.AddScoped<{SubFeature}.IRepository, {SubFeature}.Repository>();
   builder.Services.AddScoped<IValidator<{SubFeature}.Request>, {SubFeature}.Validator>();
   ```
   Match the existing `SearchMovies` lines' formatting exactly; don't reorder or disturb them.

## Step 6 — Wrap up

List every file created and every edit made to `Program.cs`. Remind the user to run `dotnet build` to confirm it compiles, and that unit tests for the new slice are handled by a separate skill (not this one).

## Reference: the real SearchMovies slice

Read `Movies.API/Features/Movies/Search/v1/*.cs` (and `Models/*.cs`) directly if you need to double check exact formatting/spacing conventions beyond what's templated above — it's the only real precedent in this codebase and templates here are derived from it.
