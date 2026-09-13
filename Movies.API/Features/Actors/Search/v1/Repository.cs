using Microsoft.EntityFrameworkCore;
using Movies.Domain.Entities;
using Movies.Infrastructure.Data;

namespace Movies.API.Features.Actors.Search.v1
{
    public partial class Search
    {
        public interface IRepository
        {
            Task<(List<Actor> Actors, int TotalCount)> SearchActorsAsync(Request request);
        }

        public class Repository(MoviesDbContext context) : IRepository
        {
            public async Task<(List<Actor> Actors, int TotalCount)> SearchActorsAsync(Request request)
            {
                var query = context.Actors.AsQueryable();

                if(!string.IsNullOrEmpty(request.SearchTerm))
                    query = query.Where(a => a.Name.Contains(request.SearchTerm));

                query = request.SortByDescending ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name);

                var totalCount = await query.CountAsync();

                var actors = await query
                    .AsNoTracking()
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                    return (actors, totalCount);
            }
        }
    }
}