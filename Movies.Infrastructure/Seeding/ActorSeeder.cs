using Movies.Domain.Entities;
using Movies.Infrastructure.Data;

namespace Movies.Infrastructure.Seeding;

public static class ActorSeeder
{
    // No source data exists for actors, so names are generated from these pools instead. A fixed
    // seed keeps the generated pool identical every time the database is recreated and reseeded.
    private const int Seed = 12345;
    private const int ActorCount = 500;

    private static readonly string[] FirstNames =
    [
        "James", "Robert", "Michael", "William", "David", "Richard", "Joseph", "Thomas", "Charles", "Daniel",
        "Matthew", "Anthony", "Mark", "Paul", "Steven", "Andrew", "Kenneth", "George", "Joshua", "Kevin",
        "Brian", "Edward", "Ronald", "Timothy", "Jason", "Jeffrey", "Ryan", "Jacob", "Gary", "Nicholas",
        "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth", "Barbara", "Susan", "Jessica", "Sarah", "Karen"
    ];

    private static readonly string[] LastNames =
    [
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez",
        "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin",
        "Lee", "Perez", "Thompson", "White", "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
        "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores"
    ];

    public static async Task<List<Actor>> SeedAsync(MoviesDbContext context)
    {
        var actors = GenerateNames().Select(name => new Actor { Name = name }).ToList();

        await context.Actors.AddRangeAsync(actors);
        await context.SaveChangesAsync();

        return actors;
    }

    private static List<string> GenerateNames()
    {
        var random = new Random(Seed);
        var names = new HashSet<string>();

        while (names.Count < ActorCount)
            names.Add($"{FirstNames[random.Next(FirstNames.Length)]} {LastNames[random.Next(LastNames.Length)]}");

        return names.ToList();
    }
}
