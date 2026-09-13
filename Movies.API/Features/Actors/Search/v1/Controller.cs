using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Common;

namespace Movies.API.Features.Actors.Search.v1
{
    [ApiController]
    [Route("api/actors/search/v1")]
    public class SearchActorsController(SearchActors.IHandler handler, IValidator<SearchActors.Request> validator)
        : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<SearchActors.Response>> SearchActorsAsync([FromQuery]SearchActors.Request request)
        {
            var validationError = await ValidateAsync(validator, request);

            if (validationError is not null)
                return validationError;

            return await handler.SearchActorsAsync(request);
        }
    }
}
