using Microsoft.AspNetCore.Mvc;
using Movies.API.Common;

namespace Movies.API.Features.Genres.GetAll.v1
{
    [ApiController]
    [Route("api/genres/get-all/v1")]
    public class GetAllGenresController(GetAllGenres.IHandler handler) : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<GetAllGenres.GenreSummary>>> GetAllGenresAsync() =>
            await handler.GetAllGenresAsync();
    }
}
