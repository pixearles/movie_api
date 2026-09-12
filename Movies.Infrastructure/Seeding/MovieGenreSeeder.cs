using Movies.Domain.Entities;
using Movies.Infrastructure.Data;

namespace Movies.Infrastructure.Seeding;

public static class MovieGenreSeeder
{
    public static async Task SeedAsync(
        MoviesDbContext context,
        IReadOnlyList<MovieCsvRecord> records,
        IReadOnlyList<Movie> movies,
        IReadOnlyDictionary<string, Genre> genresByName)
    {
        var movieGenres = new List<MovieGenre>();

        for (var i = 0; i < records.Count; i++)
        {
            var genreNames = GenreSeeder.SplitGenres(records[i]);

            foreach (var genreName in genreNames)
                movieGenres.Add(new MovieGenre { MovieId = movies[i].Id, GenreId = genresByName[genreName].Id });
        }

        await context.MovieGenres.AddRangeAsync(movieGenres);
        await context.SaveChangesAsync();
    }
}