using Microsoft.AspNetCore.Mvc;
using Moq;
using Movies.Domain.Entities;

namespace Movies.API.Features.Movies.GetMovieDetails.v1
{
    [TestClass]
    public class GetMovieDetailsHandlerTests
    {
        private static Movie CreateMovie(int id, string title, string[] genreNames, string[] actorNames) => new()
        {
            Id = id,
            Title = title,
            Overview = "An overview",
            PosterUrl = "https://example.com/poster.jpg",
            OriginalLanguage = "en",
            ReleaseDate = new DateTime(2020, 1, 1),
            VoteAverage = 7.5m,
            VoteCount = 100,
            Popularity = 10m,
            MovieGenres = genreNames.Select((name, i) => new MovieGenre
            {
                MovieId = id,
                GenreId = i,
                Genre = new Genre { Id = i, Name = name }
            }).ToList(),
            MovieActors = actorNames.Select((name, i) => new MovieActor
            {
                MovieId = id,
                ActorId = i,
                Actor = new Actor { Id = i, Name = name }
            }).ToList()
        };

        [TestMethod]
        public async Task GetMovieDetailsAsync_WhenMovieFound_MapsAllFieldsToResponse()
        {
            var movie = CreateMovie(1, "Test Movie", ["Action", "Drama"], ["Alice", "Bob"]);
            var repository = new Mock<GetMovieDetails.IRepository>();
            repository.Setup(r => r.GetMovieDetailsAsync(1)).ReturnsAsync(movie);

            var handler = new GetMovieDetails.Handler(repository.Object);
            var result = await handler.GetMovieDetailsAsync(1);

            var response = result.Value;
            Assert.IsNotNull(response);
            Assert.AreEqual(movie.Id, response.Id);
            Assert.AreEqual(movie.Title, response.Title);
            Assert.AreEqual(movie.ReleaseDate, response.ReleaseDate);
            Assert.AreEqual(movie.Overview, response.Overview);
            Assert.AreEqual(movie.PosterUrl, response.PosterUrl);
            Assert.AreEqual(movie.VoteAverage, response.AverageVote);
            Assert.AreEqual(movie.Popularity, response.Popularity);
            Assert.AreEqual(movie.OriginalLanguage, response.OriginalLanguage);
            CollectionAssert.AreEquivalent(new[] { "Action", "Drama" }, response.Genres.Select(g => g.Name).ToList());
            CollectionAssert.AreEquivalent(new[] { "Alice", "Bob" }, response.Actors.Select(a => a.Name).ToList());
        }

        [TestMethod]
        public async Task GetMovieDetailsAsync_WhenMovieNotFound_ReturnsNotFoundResult()
        {
            var repository = new Mock<GetMovieDetails.IRepository>();
            repository.Setup(r => r.GetMovieDetailsAsync(It.IsAny<int>())).ReturnsAsync((Movie?)null);

            var handler = new GetMovieDetails.Handler(repository.Object);
            var result = await handler.GetMovieDetailsAsync(1);

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task GetMovieDetailsAsync_PassesIdThroughToRepository()
        {
            var repository = new Mock<GetMovieDetails.IRepository>();
            repository.Setup(r => r.GetMovieDetailsAsync(It.IsAny<int>())).ReturnsAsync((Movie?)null);

            var handler = new GetMovieDetails.Handler(repository.Object);
            await handler.GetMovieDetailsAsync(42);

            repository.Verify(r => r.GetMovieDetailsAsync(42), Times.Once);
        }
    }

    [TestClass]
    public class GetMovieDetailsControllerTests
    {
        [TestMethod]
        public async Task GetMovieDetailsAsync_DelegatesToHandlerAndReturnsResultUnchanged()
        {
            var expectedResponse = new GetMovieDetails.Response { Id = 1, Title = "Test Movie" };
            var handler = new Mock<GetMovieDetails.IHandler>();
            handler.Setup(h => h.GetMovieDetailsAsync(1)).ReturnsAsync(expectedResponse);

            var controller = new GetMovieDetailsController(handler.Object);
            var result = await controller.GetMovieDetailsAsync(1);

            Assert.AreSame(expectedResponse, result.Value);
            handler.Verify(h => h.GetMovieDetailsAsync(1), Times.Once);
        }
    }
}
