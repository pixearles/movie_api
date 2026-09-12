using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Domain.Entities;

namespace Movies.Infrastructure.Configuration
{
    public class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder
                .HasKey(m => m.Id);
            
            builder
                .Property(m => m.Title)
                .IsRequired();

            builder
                .HasMany(m => m.MovieGenres)
                .WithOne(m => m.Movie)
                .HasForeignKey(m => m.MovieId);
            
            builder
                .HasMany(m => m.MovieActors)
                .WithOne(m => m.Movie)
                .HasForeignKey(m => m.MovieId);

            builder
                .Property(m => m.VoteAverage)
                .HasPrecision(2,1);
            
            builder
                .Property(m => m.Popularity)
                .HasPrecision(8,3);
        }       
    }
}