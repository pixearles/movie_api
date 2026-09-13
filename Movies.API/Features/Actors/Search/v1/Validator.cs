using FluentValidation;

namespace Movies.API.Features.Actors.Search.v1
{
    public partial class SearchActors
    {
        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.PageNumber)
                    .GreaterThan(0);

                RuleFor(x => x.PageSize)
                    .InclusiveBetween(1, 100);
            }
        }
    }
}
