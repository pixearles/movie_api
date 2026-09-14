using Microsoft.AspNetCore.Mvc;

namespace Movies.API.Features.Movies.Search.v1
{
    public partial class SearchMovies
    {
        public interface IHandler
        {
            Task<ActionResult<Response>> SearchMoviesAsync(Request request);
        }

        public class Handler(IRepository repository) :IHandler
        {

            public async Task<ActionResult<Response>> SearchMoviesAsync(Request request)
            {
                var (movies, totalCount) = await repository.SearchMoviesAsync(request);

                return new Response
                {
                    Movies = movies.Select(m => new MovieSummary
                    {
                        Id = m.Id,
                        Title = m.Title,
                        ReleaseDate = m.ReleaseDate,
                        PosterUrl = m.PosterUrl,
                        VoteAverage = m.VoteAverage,
                        OriginalLanguage = m.OriginalLanguage,
                        Genres = m.MovieGenres.Select(mg => mg.Genre.Name).ToList()
                    }).ToList(),
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
                };
            }
        }
    }
}