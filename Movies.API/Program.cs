using Microsoft.EntityFrameworkCore;
using Movies.Infrastructure;
using Movies.Infrastructure.Data;
using Movies.Infrastructure.Seeding;
using Movies.API.Features.Movies.Search.v1;
using Movies.API.Features.Movies.GetMovieDetails.v1;
using Movies.API.Features.Actors.Search.v1;
using Movies.API.Features.Genres.GetAll.v1;
using Movies.API.Features.Genres.Search.v1;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("MoviesDb")!);

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
          .AllowAnyHeader()
          .AllowAnyMethod()));

// feature slice registration
builder.Services.AddScoped<SearchMovies.IHandler, SearchMovies.Handler>();
builder.Services.AddScoped<SearchMovies.IRepository, SearchMovies.Repository>();
builder.Services.AddScoped<IValidator<SearchMovies.Request>, SearchMovies.Validator>();
builder.Services.AddScoped<GetMovieDetails.IHandler, GetMovieDetails.Handler>();
builder.Services.AddScoped<GetMovieDetails.IRepository, GetMovieDetails.Repository>();
builder.Services.AddScoped<SearchActors.IHandler, SearchActors.Handler>();
builder.Services.AddScoped<SearchActors.IRepository, SearchActors.Repository>();
builder.Services.AddScoped<IValidator<SearchActors.Request>, SearchActors.Validator>();
builder.Services.AddScoped<GetAllGenres.IHandler, GetAllGenres.Handler>();
builder.Services.AddScoped<GetAllGenres.IRepository, GetAllGenres.Repository>();
builder.Services.AddScoped<SearchGenres.IHandler, SearchGenres.Handler>();
builder.Services.AddScoped<SearchGenres.IRepository, SearchGenres.Repository>();
builder.Services.AddScoped<IValidator<SearchGenres.Request>, SearchGenres.Validator>();

var app = builder.Build();

await MigrateAndSeedAsync(app.Services);

if (args.Contains("migrate"))
    return;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();

app.UseCors("Frontend");

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