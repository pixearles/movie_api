using Movies.Domain.Entities;
using Movies.Infrastructure.Data;

namespace Movies.Infrastructure.Seeding;

public static class MovieSeeder
{
    public static async Task<List<Movie>> SeedAsync(MoviesDbContext context, IReadOnlyList<MovieCsvRecord> records)
    {
        var movies = records.Select(ToMovie).ToList();

        await context.Movies.AddRangeAsync(movies);
        await context.SaveChangesAsync();

        return movies;
    }

    private static Movie ToMovie(MovieCsvRecord record) => new()
    {
        Title = record.Title,
        ReleaseDate = record.ReleaseDate,
        Overview = record.Overview,
        Popularity = record.Popularity,
        VoteCount = record.VoteCount,
        VoteAverage = record.VoteAverage,
        OriginalLanguage = record.OriginalLanguage,
        PosterUrl = record.PosterUrl
    };
}