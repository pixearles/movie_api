using Microsoft.EntityFrameworkCore;
using Movies.Domain.Entities;
using Movies.Infrastructure.Data;

namespace Movies.API.Features.Movies.Search.v1
{
    public partial class SearchMovies
    {
        public interface IRepository
        {
            Task<(List<Movie> Movies, int TotalCount)>  SearchMoviesAsync(Request request);
        }

        public class Repository(MoviesDbContext context) : IRepository
        {

            public async Task<(List<Movie> Movies, int TotalCount)> SearchMoviesAsync(Request request)
            {
                var query = context.Movies.AsQueryable();

                if(!string.IsNullOrEmpty(request.SearchTerm))
                    query = query.Where(m => m.Title.Contains(request.SearchTerm));
                
                if(request.Genres is {Count: > 0})
                    query = query.Where(m => m.MovieGenres.Any(mg => request.Genres.Contains(mg.GenreId)));
                
                if(request.Actors is {Count: > 0})
                    query = query.Where(m => m.MovieActors.Count(ma => request.Actors.Contains(ma.ActorId)) == request.Actors.Count);
                
                query = request.SortBy switch
                {
                  "releaseDate" => request.SortByDescending ? query.OrderByDescending(m=> m.ReleaseDate) : query.OrderBy(m=> m.ReleaseDate),
                    _ => request.SortByDescending ? query = query.OrderByDescending(m=>m.Title) : query.OrderBy(m=> m.Title)
                };

                var totalCount = await query.CountAsync();
                
                var movies = await query
                    .AsNoTracking()
                    .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                    .Skip((request.PageNumber -1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return (movies, totalCount);
            }
        }
    }
}