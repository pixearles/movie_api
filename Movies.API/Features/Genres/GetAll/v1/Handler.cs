using Microsoft.AspNetCore.Mvc;

namespace Movies.API.Features.Genres.GetAll.v1
{
    public partial class GetAllGenres
    {
        public interface IHandler
        {
            Task<ActionResult<List<GenreSummary>>> GetAllGenresAsync();
        }

        public class Handler(IRepository repository) : IHandler
        {
            public async Task<ActionResult<List<GenreSummary>>> GetAllGenresAsync()
            {
                var genres = await repository.GetAllGenresAsync();

                return genres.Select(g => new GenreSummary
                {
                    Id = g.Id,
                    Name = g.Name
                }).ToList();
            }
        }
    }
}
