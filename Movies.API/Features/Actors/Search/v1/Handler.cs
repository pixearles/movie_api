using Microsoft.AspNetCore.Mvc;

namespace Movies.API.Features.Actors.Search.v1
{
    public partial class Search
    {
        public interface IHandler
        {
            Task<ActionResult<Response>> SearchActorsAsync(Request request);
        }

        public class Handler(IRepository repository) : IHandler
        {
            public async Task<ActionResult<Response>> SearchActorsAsync(Request request)
            {
                var (actors, totalCount) = await repository.SearchActorsAsync(request);

                return new Response
                {
                    Actors = actors.Select(a => new ActorSummary
                    {
                        Id = a.Id,
                        Name = a.Name
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
