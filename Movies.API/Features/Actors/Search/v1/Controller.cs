using Microsoft.AspNetCore.Mvc;
using Movies.API.Common;

namespace Movies.API.Features.Actors.Search.v1
{
    [ApiController]
    [Route("api/actors/search/v1")]
    public class SearchController(Search.IHandler handler) : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<Search.Response>> SearchAsync([FromQuery]Search.Request request) =>
            await handler.SearchActorsAsync(request);
    }
}
