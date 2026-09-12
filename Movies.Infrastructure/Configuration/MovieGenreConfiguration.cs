using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Domain.Entities;

namespace Movies.Infrastructure.Configuration
{
    public class MovieGenreConfiguration : IEntityTypeConfiguration<MovieGenre>
    {
        public void Configure(EntityTypeBuilder<MovieGenre> builder)
        {
            builder
                .HasKey(mg => new { mg.MovieId, mg.GenreId });

            builder
                .HasOne(mg => mg.Genre)
                .WithMany()
                .HasForeignKey(mg => mg.GenreId);
        }
    }
}