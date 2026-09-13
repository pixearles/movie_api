using Microsoft.EntityFrameworkCore;
using Movies.Domain.Entities;
using Movies.Infrastructure.Data;

namespace Movies.API.Features.Genres.GetAll.v1
{
    public partial class GetAllGenres
    {
        public interface IRepository
        {
            Task<List<Genre>> GetAllGenresAsync();
        }

        public class Repository(MoviesDbContext context) : IRepository
        {
            public async Task<List<Genre>> GetAllGenresAsync() =>
                await context.Genres.AsNoTracking().OrderBy(g => g.Name).ToListAsync();
        }
    }
}
