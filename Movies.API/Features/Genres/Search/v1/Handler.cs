using Microsoft.AspNetCore.Mvc;

namespace Movies.API.Features.Genres.Search.v1
{
    public partial class SearchGenres
    {
        public interface IHandler
        {
            Task<ActionResult<Response>> SearchGenresAsync(Request request);
        }

        public class Handler(IRepository repository) : IHandler
        {
            public async Task<ActionResult<Response>> SearchGenresAsync(Request request)
            {
                var (genres, totalCount) = await repository.SearchGenresAsync(request);

                return new Response
                {
                    Genres = genres.Select(g => new GenreDetails
                    {
                        Id = g.Id,
                        Name = g.Name
                    }).ToList(),
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
                };
            }
        }
    }
}