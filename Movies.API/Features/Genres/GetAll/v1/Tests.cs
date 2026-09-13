using Moq;
using Movies.Domain.Entities;

namespace Movies.API.Features.Genres.GetAll.v1
{
    [TestClass]
    public class GetAllGenresHandlerTests
    {
        [TestMethod]
        public async Task GetAllGenresAsync_MapsRepositoryGenresToGenreSummaries()
        {
            var genres = new List<Genre>
            {
                new() { Id = 1, Name = "Action" },
                new() { Id = 2, Name = "Comedy" }
            };
            var repository = new Mock<GetAllGenres.IRepository>();
            repository.Setup(r => r.GetAllGenresAsync()).ReturnsAsync(genres);

            var handler = new GetAllGenres.Handler(repository.Object);
            var result = await handler.GetAllGenresAsync();

            var summaries = result.Value!;
            Assert.AreEqual(2, summaries.Count);
            Assert.AreEqual(1, summaries[0].Id);
            Assert.AreEqual("Action", summaries[0].Name);
            Assert.AreEqual(2, summaries[1].Id);
            Assert.AreEqual("Comedy", summaries[1].Name);
        }

        [TestMethod]
        public async Task GetAllGenresAsync_WhenNoGenres_ReturnsEmptyList()
        {
            var repository = new Mock<GetAllGenres.IRepository>();
            repository.Setup(r => r.GetAllGenresAsync()).ReturnsAsync([]);

            var handler = new GetAllGenres.Handler(repository.Object);
            var result = await handler.GetAllGenresAsync();

            Assert.AreEqual(0, result.Value!.Count);
        }

        [TestMethod]
        public async Task GetAllGenresAsync_CallsRepositoryOnce()
        {
            var repository = new Mock<GetAllGenres.IRepository>();
            repository.Setup(r => r.GetAllGenresAsync()).ReturnsAsync([]);

            var handler = new GetAllGenres.Handler(repository.Object);
            await handler.GetAllGenresAsync();

            repository.Verify(r => r.GetAllGenresAsync(), Times.Once);
        }
    }

    [TestClass]
    public class GetAllGenresControllerTests
    {
        [TestMethod]
        public async Task GetAllGenresAsync_ReturnsHandlerResultAndCallsHandlerOnce()
        {
            var expected = new List<GetAllGenres.GenreSummary> { new() { Id = 1, Name = "Action" } };
            var handler = new Mock<GetAllGenres.IHandler>();
            handler.Setup(h => h.GetAllGenresAsync()).ReturnsAsync(expected);

            var controller = new GetAllGenresController(handler.Object);
            var result = await controller.GetAllGenresAsync();

            Assert.AreSame(expected, result.Value);
            handler.Verify(h => h.GetAllGenresAsync(), Times.Once);
        }
    }
}
