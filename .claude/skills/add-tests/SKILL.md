---
name: add-tests
description: Generate MSTest + Moq unit tests (Controller/Validator/Handler) for an existing, already-implemented vertical-slice feature in the Movies API. Use when the user wants tests added for a specific slice under Movies.API/Features/. Pairs with the add-slice skill, which scaffolds the slice itself but deliberately leaves business logic as TODO stubs.
---

# Add tests for an existing slice

Generates a `Tests.cs` for one vertical-slice sub-feature under `Movies.API/Features/`, by reading that slice's actual `Controller.cs`/`Validator.cs`/`Handler.cs` and deriving real tests from what's actually implemented — not blind templating. This is the companion to the `add-slice` skill: `add-slice` scaffolds structure and leaves `Validator.cs`/`Handler.cs` as `// TODO`/`NotImplementedException` stubs on purpose (it doesn't guess business logic), so there's nothing real to test until the user fills those in. This skill picks up from there.

Always interactive, operates on exactly one slice per run.

## Non-obvious things to bake in (found while hand-writing `SearchMovies`'s tests — don't rediscover these)

- `ApiControllerBase.ValidateAsync` (`Movies.API/Common/ApiControllerBase.cs`) calls FluentValidation's `IValidator<T>.ValidateAsync(T instance, CancellationToken)` overload directly. Mocking the `ValidationContext<T>` overload instead (`v.ValidateAsync(It.IsAny<ValidationContext<TRequest>>(), ...)`) silently matches nothing — Moq returns `null`, and `ApiControllerBase.ValidateAsync` throws `NullReferenceException` when it awaits that. Always mock: `validator.Setup(v => v.ValidateAsync(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))`.
- This project's `MSTest` version flags `[DataTestMethod]` as obsolete (`MSTEST0044`). Use plain `[TestMethod]` together with `[DataRow]` — no separate `[DataTestMethod]` attribute.
- A controller that calls `ValidateAsync`/`ValidationProblem` needs a `ControllerContext` with a DI-backed `HttpContext` registering `ProblemDetailsFactory`, or `ValidationProblem()` throws `NullReferenceException` in a test. Use this helper pattern:
  ```csharp
  private static {SubFeature}Controller CreateController({SubFeature}.IHandler handler, IValidator<{SubFeature}.Request> validator)
  {
      var services = new ServiceCollection();
      services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
      services.AddSingleton(Options.Create(new ApiBehaviorOptions()));
      var provider = services.BuildServiceProvider();

      return new {SubFeature}Controller(handler, validator)
      {
          ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = provider } }
      };
  }
  ```
  A controller with no validator doesn't need any of this — it never calls `ValidateAsync`.

## Step 1 — Ask which slice to target

Ask conversationally which slice to generate tests for (feature + sub-feature name, e.g. "Movies / GetMovieDetails") — do not scan `Movies.API/Features/` and offer a pick-list. Once named, verify `Movies.API/Features/{Feature}/{SubFeature}/v1/` actually exists; if not, say so and ask again rather than guessing at a close match.

## Step 2 — Check completeness

Read the slice's `Validator.cs` (if present) and `Handler.cs`. Check for stub markers: `// TODO`, `NotImplementedException`.

If either is still a stub:
- Summarize what's incomplete (which file, what's missing).
- Ask via `AskUserQuestion` whether to stop so the user can finish the implementation first (recommended/default), or continue anyway — generating tests only for the parts that are real and skipping the stubbed piece.

`Repository.cs`'s completeness never gates anything — `HandlerTests` mocks `IRepository` by its interface shape alone, so it doesn't matter whether `Repository.cs` itself still has a `// TODO` body.

## Step 3 — Confirm the plan

Summarize which test classes will be generated before writing anything: `{SubFeature}ControllerTests` (always), `{SubFeature}ValidatorTests` (only if `Validator.cs` has real rules), `{SubFeature}HandlerTests` (only if `Handler.cs` has real logic).

## Step 4 — Guard rail

If `Tests.cs` already exists for this slice, stop and ask before overwriting rather than clobbering it.

## Step 5 — Generate `Tests.cs`

Single file at `Movies.API/Features/{Feature}/{SubFeature}/v1/Tests.cs`, containing the applicable `[TestClass]` classes below, in namespace `Movies.API.Features.{Feature}.{SubFeature}.v1`. Read the slice's actual `Controller.cs`, `Validator.cs`, `Handler.cs`, and `Models/*.cs` first — every test case below must be derived from what that code actually does, not guessed.

### `{SubFeature}ControllerTests` — always generated

Mock `{SubFeature}.IHandler` and, if the controller has one, `IValidator<{SubFeature}.Request>`. Detect validator presence by checking whether `Controller.cs` calls `ValidateAsync`.

- **With a validator** (use the `CreateController` helper from above):
  - Invalid request (validator mock returns a `ValidationResult` with one `ValidationFailure`) → result is an `ObjectResult` with `StatusCode == 400`; `handler.{Method}Async` verified `Times.Never`.
  - Valid request (validator mock returns `new ValidationResult()`) → controller returns exactly what the handler mock returned (`Assert.AreSame`); `handler.{Method}Async` verified `Times.Once` with the same request/params.
- **Without a validator**: one test — controller delegates to the handler and returns its result unchanged. No `ControllerContext` setup needed.

### `{SubFeature}ValidatorTests` — only if `Validator.cs` has real `RuleFor(...)` rules

No Moq needed (`Validator` has no dependencies). Use `FluentValidation.TestHelper` (`using FluentValidation.TestHelper;` — ships inside the `FluentValidation` package already referenced): `validator.TestValidate(request)` then `.ShouldHaveValidationErrorFor(...)` / `.ShouldNotHaveValidationErrorFor(...)`.

For each rule actually present in `Validator.cs`, generate matching `[TestMethod]` + `[DataRow]` boundary cases — mirror the style in `Movies.API/Features/Movies/Search/v1/Tests.cs`'s `SearchMoviesValidatorTests`:
- A numeric range/comparison rule → one test with `[DataRow]`s just outside the valid boundary (error expected), one with `[DataRow]`s at/inside the boundary (no error expected).
- A `Must(...)`/set-membership rule (e.g. "must be one of X/Y", case-insensitive) → one test per accepted value plus null (if allowed), one test per rejected value.
- A duplicate-detection rule → one test with a null/empty/no-duplicates collection (no error), one with duplicates (error).

### `{SubFeature}HandlerTests` — only if `Handler.cs` has real logic

Mock `{SubFeature}.IRepository`. Derive test cases from what the handler actually does — read its real mapping/branching code:
- Field-by-field mapping correctness from whatever the repository returns to the `Response` shape.
- Any computed values (pagination math, counts, aggregates) — use `[TestMethod]` + `[DataRow]` across boundary inputs, same as `SearchMoviesHandlerTests.SearchMoviesAsync_ComputesTotalPagesCorrectly`.
- Edge cases the handler actually branches on (empty repository result, `null` → e.g. a `NotFoundResult`, etc.) — check `Handler.cs` for `if` branches and cover each one.
- The exact request/params passed to `Handler.{Method}Async` are forwarded unchanged to `repository.{Method}Async` (Moq `Verify`).

If the relevant domain entity (`Movies.Domain.Entities.*`) has non-nullable properties with no default values (check for `CS8618` build warnings on it, as `Movie` has), add one small private `Create{Entity}(...)` test-data builder in the test class — see `SearchMoviesHandlerTests.CreateMovie` for the pattern.

## Step 6 — Build and run

Run `dotnet build Movies.API/Movies.API.csproj`, fix any compile errors, then `dotnet test Movies.API/Movies.API.csproj --filter "FullyQualifiedName~{SubFeature}"`. Fix any failing assertions before finishing — this is exactly how a real mock-overload bug was caught while hand-writing the `SearchMovies` tests, so don't skip it.

## Step 7 — Wrap up

List what was generated, and call out anything skipped and why (e.g. "`{SubFeature}ValidatorTests` skipped — `Validator.cs` still has a TODO stub"). Note that `Repository.cs` is never covered by this skill — its EF Core query logic needs an EF Core InMemory-provider test or a real-database integration test to verify meaningfully, which is a separate, bigger undertaking.

## Reference

`Movies.API/Features/Movies/Search/v1/Tests.cs` is the real, hand-written precedent every template above is derived from — read it directly if anything here is ambiguous.
