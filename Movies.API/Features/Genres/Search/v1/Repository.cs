using Microsoft.EntityFrameworkCore;
using Movies.Domain.Entities;
using Movies.Infrastructure.Data;

namespace Movies.API.Features.Genres.Search.v1
{
    public partial class SearchGenres
    {
        public interface IRepository
        {
            Task<(List<Genre> Genres, int TotalCount)> SearchGenresAsync(Request request);
        }

        public class Repository(MoviesDbContext context) : IRepository
        {
            public async Task<(List<Genre> Genres, int TotalCount)> SearchGenresAsync(Request request)
            {
                var query = context.Genres.AsQueryable();

                if(!string.IsNullOrEmpty(request.SearchTerm))
                    query = query.Where(m => m.Name.Contains(request.SearchTerm));

                query = request.SortByDescending ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name);

                var totalCount = await query.CountAsync();

                var genres = await query
                    .AsNoTracking()
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                    return (genres, totalCount);
            }
        }
    }
}
