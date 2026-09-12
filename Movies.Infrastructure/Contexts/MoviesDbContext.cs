using Microsoft.EntityFrameworkCore;
using Movies.Domain.Entities;
using Movies.Infrastructure.Configuration;

namespace Movies.Infrastructure.Data;

public class MoviesDbContext : DbContext
{
    public MoviesDbContext(DbContextOptions<MoviesDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies {get;set;}
    public DbSet<Genre> Genres {get;set;}
    public DbSet<MovieGenre> MovieGenres {get;set;}
    public DbSet<Actor> Actors {get;set;}
    public DbSet<MovieActor> MovieActors {get;set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new MovieConfiguration());
        modelBuilder.ApplyConfiguration(new GenreConfiguration());
        modelBuilder.ApplyConfiguration(new MovieGenreConfiguration());
    }
}