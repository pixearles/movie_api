using Microsoft.AspNetCore.Mvc;
using Movies.API.Common;

namespace Movies.API.Features.Movies.GetMovieDetails.v1
{
    [ApiController]
    [Route("api/movies/get-movie-details/v1")]
    public class GetMovieDetailsController(GetMovieDetails.IHandler handler) : ApiControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetMovieDetails.Response>> GetMovieDetailsAsync(int id) =>
            await handler.GetMovieDetailsAsync(id);
    }
}
