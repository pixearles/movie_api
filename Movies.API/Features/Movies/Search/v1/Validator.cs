using FluentValidation;

namespace Movies.API.Features.Movies.Search.v1
{
    public partial class SearchMovies
    {
        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.PageNumber)
                    .GreaterThan(0);

                RuleFor(x => x.PageSize)
                    .InclusiveBetween(1, 100);

                RuleFor(x => x.SortBy)
                    .Must(sortBy => sortBy is null
                        || sortBy.Equals("title", StringComparison.OrdinalIgnoreCase)
                        || sortBy.Equals("releaseDate", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("SortBy must be 'title' or 'releaseDate'.");

                RuleFor(x => x.Actors)
                    .Must(actors => actors is null || actors.Count == actors.Distinct().Count())
                    .WithMessage("Duplicate actor ids are not allowed.");
            }
        }
    }
}