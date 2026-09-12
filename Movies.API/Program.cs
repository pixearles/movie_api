using Microsoft.EntityFrameworkCore;
using Movies.Infrastructure;
using Movies.Infrastructure.Data;
using Movies.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("MoviesDb")!);

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
