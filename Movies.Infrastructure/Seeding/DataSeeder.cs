using System.Globalization;
using CsvHelper;
using Movies.Infrastructure.Data;

namespace Movies.Infrastructure.Seeding;

public static class DataSeeder
{
    public static async Task SeedAsync(MoviesDbContext context)
    {
        var records = ReadRecords();

        await using var transaction = await context.Database.BeginTransactionAsync();

        var genresByName = await GenreSeeder.SeedAsync(context, records);
        var movies = await MovieSeeder.SeedAsync(context, records);
        await MovieGenreSeeder.SeedAsync(context, records, movies, genresByName);

        var actors = await ActorSeeder.SeedAsync(context);
        await MovieActorSeeder.SeedAsync(context, movies, actors);

        await transaction.CommitAsync();
    }

    private static List<MovieCsvRecord> ReadRecords()
    {
        var csvPath = Path.Combine(AppContext.BaseDirectory, "OriginalData", "mymoviedb.csv");
        var csvText = File.ReadAllText(csvPath).Replace("\r\n", "\n").Replace("\r", " ");

        using var reader = new StringReader(csvText);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        return csv.GetRecords<MovieCsvRecord>().ToList();
    }
}