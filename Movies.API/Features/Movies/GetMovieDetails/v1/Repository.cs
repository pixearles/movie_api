using Microsoft.EntityFrameworkCore;
using Movies.Domain.Entities;
using Movies.Infrastructure.Data;

namespace Movies.API.Features.Movies.GetMovieDetails.v1
{
    public partial class GetMovieDetails
    {
        public interface IRepository
        {
            Task<Movie?> GetMovieDetailsAsync(int id);
        }

        public class Repository(MoviesDbContext context) : IRepository
        {
            public async Task<Movie?> GetMovieDetailsAsync(int id) =>
                await context.Movies
                    .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                    .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                    .AsSplitQuery()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
