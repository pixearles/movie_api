using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Common;

namespace Movies.API.Features.Genres.Search.v1
{
    [ApiController]
    [Route("api/genres/search/v1")]
    public class SearchGenresController(SearchGenres.IHandler handler, IValidator<SearchGenres.Request> validator)
        : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<SearchGenres.Response>> SearchGenresAsync([FromQuery]SearchGenres.Request request)
        {
            var validationError = await ValidateAsync(validator, request);

            if (validationError is not null)
                return validationError;

            return await handler.SearchGenresAsync(request);
        }
    }
}
