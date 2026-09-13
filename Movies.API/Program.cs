using Microsoft.EntityFrameworkCore;
using Movies.Infrastructure;
using Movies.Infrastructure.Data;
using Movies.Infrastructure.Seeding;
using Movies.API.Features.Movies.Search.v1;
using Movies.API.Features.Actors.Search.v1;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("MoviesDb")!);

// feature slice registration
builder.Services.AddScoped<SearchMovies.IHandler, SearchMovies.Handler>();
builder.Services.AddScoped<SearchMovies.IRepository, SearchMovies.Repository>();
builder.Services.AddScoped<IValidator<SearchMovies.Request>, SearchMovies.Validator>();
builder.Services.AddScoped<Search.IHandler, Search.Handler>();
builder.Services.AddScoped<Search.IRepository, Search.Repository>();

var app = builder.Build();

await MigrateAndSeedAsync(app.Services);

if (args.Contains("migrate"))
    return;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task MigrateAndSeedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MoviesDbContext>();

    var isNewDatabase = !(await context.Database.GetAppliedMigrationsAsync()).Any();

    await context.Database.MigrateAsync();

    if (isNewDatabase)
        await DataSeeder.SeedAsync(context);
}