using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Movies.Domain.Entities;

namespace Movies.API.Features.Movies.Search.v1
{
    [TestClass]
    public class SearchMoviesValidatorTests
    {
        private readonly SearchMovies.Validator _validator = new();

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public void PageNumber_WhenNotPositive_HasValidationError(int pageNumber)
        {
            var result = _validator.TestValidate(new SearchMovies.Request { PageNumber = pageNumber });

            result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        }

        [TestMethod]
        public void PageNumber_WhenPositive_HasNoValidationError()
        {
            var result = _validator.TestValidate(new SearchMovies.Request { PageNumber = 1 });

            result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(101)]
        public void PageSize_WhenOutsideInclusiveRange_HasValidationError(int pageSize)
        {
            var result = _validator.TestValidate(new SearchMovies.Request { PageSize = pageSize });

            result.ShouldHaveValidationErrorFor(x => x.PageSize);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(50)]
        [DataRow(100)]
        public void PageSize_WhenWithinInclusiveRange_HasNoValidationError(int pageSize)
        {
            var result = _validator.TestValidate(new SearchMovies.Request { PageSize = pageSize });

            result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("title")]
        [DataRow("TITLE")]
        [DataRow("releaseDate")]
        [DataRow("RELEASEDATE")]
        public void SortBy_WhenNullOrRecognisedValue_HasNoValidationError(string? sortBy)
        {
            var result = _validator.TestValidate(new SearchMovies.Request { SortBy = sortBy });

            result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
        }

        [TestMethod]
        [DataRow("popularity")]
        [DataRow("random")]
        public void SortBy_WhenUnrecognisedValue_HasValidationError(string sortBy)
        {
            var result = _validator.TestValidate(new SearchMovies.Request { SortBy = sortBy });

            result.ShouldHaveValidationErrorFor(x => x.SortBy);
        }

        [TestMethod]
        public void Actors_WhenNull_HasNoValidationError()
        {
            var result = _validator.TestValidate(new SearchMovies.Request { Actors = null });

            result.ShouldNotHaveValidationErrorFor(x => x.Actors);
        }

        [TestMethod]
        public void Actors_WhenNoDuplicates_HasNoValidationError()
        {
            var result = _validator.TestValidate(new SearchMovies.Request { Actors = [1, 2, 3] });

            result.ShouldNotHaveValidationErrorFor(x => x.Actors);
        }

        [TestMethod]
        public void Actors_WhenContainsDuplicates_HasValidationError()
        {
            var result = _validator.TestValidate(new SearchMovies.Request { Actors = [1, 1, 2] });

            result.ShouldHaveValidationErrorFor(x => x.Actors);
        }
    }

    [TestClass]
    public class SearchMoviesHandlerTests
    {
        private static Movie CreateMovie(int id, string title, params string[] genreNames) => new()
        {
            Id = id,
            Title = title,
            Overview = string.Empty,
            PosterUrl = string.Empty,
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
            MovieActors = []
        };

        [TestMethod]
        public async Task SearchMoviesAsync_MapsRepositoryMoviesToMovieSummaries()
        {
            var movie = CreateMovie(1, "Test Movie", "Action", "Drama");
            var repository = new Mock<SearchMovies.IRepository>();
            repository.Setup(r => r.SearchMoviesAsync(It.IsAny<SearchMovies.Request>()))
                .ReturnsAsync(([movie], 1));

            var handler = new SearchMovies.Handler(repository.Object);
            var result = await handler.SearchMoviesAsync(new SearchMovies.Request());

            var summary = result.Value!.Movies.Single();
            Assert.AreEqual(movie.Id, summary.Id);
            Assert.AreEqual(movie.Title, summary.Title);
            Assert.AreEqual(movie.ReleaseDate, summary.ReleaseDate);
            Assert.AreEqual(movie.PosterUrl, summary.PosterUrl);
            Assert.AreEqual(movie.VoteAverage, summary.VoteAverage);
            Assert.AreEqual(movie.OriginalLanguage, summary.OriginalLanguage);
            CollectionAssert.AreEquivalent(new[] { "Action", "Drama" }, summary.Genres);
        }

        [TestMethod]
        [DataRow(0, 20, 0)]
        [DataRow(1, 20, 1)]
        [DataRow(20, 20, 1)]
        [DataRow(21, 20, 2)]
        [DataRow(100, 33, 4)]
        public async Task SearchMoviesAsync_ComputesTotalPagesCorrectly(int totalCount, int pageSize, int expectedTotalPages)
        {
            var repository = new Mock<SearchMovies.IRepository>();
            repository.Setup(r => r.SearchMoviesAsync(It.IsAny<SearchMovies.Request>()))
                .ReturnsAsync(([], totalCount));

            var handler = new SearchMovies.Handler(repository.Object);
            var result = await handler.SearchMoviesAsync(new SearchMovies.Request { PageSize = pageSize });

            Assert.AreEqual(expectedTotalPages, result.Value!.TotalPages);
        }

        [TestMethod]
        public async Task SearchMoviesAsync_EchoesTotalCountAndRequestPaging()
        {
            var repository = new Mock<SearchMovies.IRepository>();
            repository.Setup(r => r.SearchMoviesAsync(It.IsAny<SearchMovies.Request>()))
                .ReturnsAsync(([], 42));

            var handler = new SearchMovies.Handler(repository.Object);
            var request = new SearchMovies.Request { PageNumber = 3, PageSize = 10 };
            var result = await handler.SearchMoviesAsync(request);

            Assert.AreEqual(42, result.Value!.TotalCount);
            Assert.AreEqual(3, result.Value!.PageNumber);
            Assert.AreEqual(10, result.Value!.PageSize);
        }

        [TestMethod]
        public async Task SearchMoviesAsync_WhenNoMoviesFound_ReturnsEmptyMoviesListWithZeroTotalPages()
        {
            var repository = new Mock<SearchMovies.IRepository>();
            repository.Setup(r => r.SearchMoviesAsync(It.IsAny<SearchMovies.Request>()))
                .ReturnsAsync(([], 0));

            var handler = new SearchMovies.Handler(repository.Object);
            var result = await handler.SearchMoviesAsync(new SearchMovies.Request());

            Assert.AreEqual(0, result.Value!.Movies.Count);
            Assert.AreEqual(0, result.Value!.TotalPages);
        }

        [TestMethod]
        public async Task SearchMoviesAsync_PassesRequestThroughToRepository()
        {
            var repository = new Mock<SearchMovies.IRepository>();
            repository.Setup(r => r.SearchMoviesAsync(It.IsAny<SearchMovies.Request>()))
                .ReturnsAsync(([], 0));

            var handler = new SearchMovies.Handler(repository.Object);
            var request = new SearchMovies.Request { SearchTerm = "matrix" };
            await handler.SearchMoviesAsync(request);

            repository.Verify(r => r.SearchMoviesAsync(request), Times.Once);
        }
    }

    [TestClass]
    public class SearchMoviesControllerTests
    {
        private static SearchMoviesController CreateController(SearchMovies.IHandler handler, IValidator<SearchMovies.Request> validator)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
            services.AddSingleton(Options.Create(new ApiBehaviorOptions()));
            var provider = services.BuildServiceProvider();

            return new SearchMoviesController(handler, validator)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { RequestServices = provider }
                }
            };
        }

        [TestMethod]
        public async Task SearchMoviesAsync_WhenValidationFails_ReturnsValidationProblemAndDoesNotCallHandler()
        {
            var handler = new Mock<SearchMovies.IHandler>();
            var validator = new Mock<IValidator<SearchMovies.Request>>();
            validator.Setup(v => v.ValidateAsync(It.IsAny<SearchMovies.Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult([new ValidationFailure("PageNumber", "must be greater than 0")]));

            var controller = CreateController(handler.Object, validator.Object);
            var result = await controller.SearchMoviesAsync(new SearchMovies.Request());

            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(400, objectResult.StatusCode);
            handler.Verify(h => h.SearchMoviesAsync(It.IsAny<SearchMovies.Request>()), Times.Never);
        }

        [TestMethod]
        public async Task SearchMoviesAsync_WhenValidationSucceeds_ReturnsHandlerResultAndCallsHandlerOnce()
        {
            var expectedResponse = new SearchMovies.Response { TotalCount = 5 };
            var handler = new Mock<SearchMovies.IHandler>();
            var request = new SearchMovies.Request();
            handler.Setup(h => h.SearchMoviesAsync(request)).ReturnsAsync(expectedResponse);

            var validator = new Mock<IValidator<SearchMovies.Request>>();
            validator.Setup(v => v.ValidateAsync(It.IsAny<SearchMovies.Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var controller = CreateController(handler.Object, validator.Object);
            var result = await controller.SearchMoviesAsync(request);

            Assert.AreSame(expectedResponse, result.Value);
            handler.Verify(h => h.SearchMoviesAsync(request), Times.Once);
        }
    }
}
