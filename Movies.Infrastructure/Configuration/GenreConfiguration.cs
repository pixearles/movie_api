using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Movies.Domain.Entities;

namespace Movies.Infrastructure.Configuration
{
    public class GenreConfiguration : IEntityTypeConfiguration<Genre>
    {
        public void Configure(EntityTypeBuilder<Genre> builder)
        {
            builder
                .HasKey(g=> g.Id);

            builder
                .Property(g => g.Name)
                .IsRequired();
            
            builder
                .HasIndex(g => g.Name)
                .IsUnique();
        }
    }
}