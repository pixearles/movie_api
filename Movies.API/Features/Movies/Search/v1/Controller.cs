using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Common;

namespace Movies.API.Features.Movies.Search.v1
{
    [ApiController]
    [Route("api/movies/search/v1")]
    public class SearchMoviesController(SearchMovies.IHandler handler, IValidator<SearchMovies.Request> validator) 
        : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<SearchMovies.Response>> SearchMoviesAsync([FromQuery]SearchMovies.Request request)
        {
            var validationError = await ValidateAsync(validator, request);
            
            if (validationError is not null)
                return validationError;

            return await handler.SearchMoviesAsync(request);
        }
    }
}