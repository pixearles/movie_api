using Movies.Domain.Entities;
using Movies.Infrastructure.Data;

namespace Movies.Infrastructure.Seeding;

public static class MovieActorSeeder
{
    private const int Seed = 67890;
    private const int MinCastSize = 3;
    private const int MaxCastSize = 6;

    public static async Task SeedAsync(MoviesDbContext context, IReadOnlyList<Movie> movies, IReadOnlyList<Actor> actors)
    {
        var random = new Random(Seed);
        var movieActors = new List<MovieActor>();
        var usedActorIds = new HashSet<int>();

        foreach (var movie in movies)
        {
            var castSize = random.Next(MinCastSize, MaxCastSize + 1);

            foreach (var actor in PickRandomActors(actors, castSize, random))
            {
                movieActors.Add(new MovieActor { MovieId = movie.Id, ActorId = actor.Id });
                usedActorIds.Add(actor.Id);
            }
        }

        // Guarantee every actor is credited in at least one movie, even though random chance
        // already makes an unused actor astronomically unlikely at this pool size.
        foreach (var actor in actors.Where(actor => !usedActorIds.Contains(actor.Id)))
            movieActors.Add(new MovieActor { MovieId = movies[random.Next(movies.Count)].Id, ActorId = actor.Id });

        await context.MovieActors.AddRangeAsync(movieActors);
        await context.SaveChangesAsync();
    }

    private static List<Actor> PickRandomActors(IReadOnlyList<Actor> actors, int count, Random random)
    {
        var pickedIds = new HashSet<int>();
        var picked = new List<Actor>();

        while (picked.Count < count)
        {
            var actor = actors[random.Next(actors.Count)];

            if (pickedIds.Add(actor.Id))
                picked.Add(actor);
        }

        return picked;
    }
}
