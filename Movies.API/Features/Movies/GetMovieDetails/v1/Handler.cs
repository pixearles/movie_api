using Microsoft.AspNetCore.Mvc;

namespace Movies.API.Features.Movies.GetMovieDetails.v1
{
    public partial class GetMovieDetails
    {
        public interface IHandler
        {
            Task<ActionResult<Response>> GetMovieDetailsAsync(int id);
        }

        public class Handler(IRepository repository) : IHandler
        {
            public async Task<ActionResult<Response>> GetMovieDetailsAsync(int id)
            {
                var movie = await repository.GetMovieDetailsAsync(id);

                if (movie is null)
                    return new NotFoundResult();

                return new Response
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    ReleaseDate = movie.ReleaseDate,
                    Overview = movie.Overview,
                    PosterUrl = movie.PosterUrl,
                    AverageVote = movie.VoteAverage,
                    Popularity = movie.Popularity,
                    OriginalLanguage = movie.OriginalLanguage,
                    Genres = movie.MovieGenres.Select(mg => new GenreRef
                    {
                        Id = mg.Genre.Id,
                        Name = mg.Genre.Name
                    }).ToList(),
                    Actors = movie.MovieActors.Select(ma => new ActorRef
                    {
                        Id = ma.Actor.Id,
                        Name = ma.Actor.Name
                    }).ToList()
                };
            }
        }
    }
}
