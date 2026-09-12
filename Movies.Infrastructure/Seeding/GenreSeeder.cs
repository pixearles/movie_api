using Movies.Domain.Entities;
using Movies.Infrastructure.Data;

namespace Movies.Infrastructure.Seeding;

public static class GenreSeeder
{
    public static async Task<Dictionary<string, Genre>> SeedAsync(MoviesDbContext context, IReadOnlyCollection<MovieCsvRecord> records)
    {
        var genreNames = records
            .SelectMany(SplitGenres)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name);

        var genres = genreNames.Select(name => new Genre { Name = name }).ToList();

        await context.Genres.AddRangeAsync(genres);
        await context.SaveChangesAsync();

        return genres.ToDictionary(genre => genre.Name, StringComparer.OrdinalIgnoreCase);
    }

    public static string[] SplitGenres(MovieCsvRecord record) =>
        record.Genre.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}