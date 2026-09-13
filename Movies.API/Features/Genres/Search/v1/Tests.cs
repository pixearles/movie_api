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

namespace Movies.API.Features.Genres.Search.v1
{
    [TestClass]
    public class SearchGenresValidatorTests
    {
        private readonly SearchGenres.Validator _validator = new();

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public void PageNumber_WhenNotPositive_HasValidationError(int pageNumber)
        {
            var result = _validator.TestValidate(new SearchGenres.Request { PageNumber = pageNumber });

            result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        }

        [TestMethod]
        public void PageNumber_WhenPositive_HasNoValidationError()
        {
            var result = _validator.TestValidate(new SearchGenres.Request { PageNumber = 1 });

            result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(101)]
        public void PageSize_WhenOutsideInclusiveRange_HasValidationError(int pageSize)
        {
            var result = _validator.TestValidate(new SearchGenres.Request { PageSize = pageSize });

            result.ShouldHaveValidationErrorFor(x => x.PageSize);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(50)]
        [DataRow(100)]
        public void PageSize_WhenWithinInclusiveRange_HasNoValidationError(int pageSize)
        {
            var result = _validator.TestValidate(new SearchGenres.Request { PageSize = pageSize });

            result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
        }
    }

    [TestClass]
    public class SearchGenresHandlerTests
    {
        private static Genre CreateGenre(int id, string name) => new()
        {
            Id = id,
            Name = name
        };

        [TestMethod]
        public async Task SearchGenresAsync_MapsRepositoryGenresToGenreDetails()
        {
            var genre = CreateGenre(1, "Action");
            var repository = new Mock<SearchGenres.IRepository>();
            repository.Setup(r => r.SearchGenresAsync(It.IsAny<SearchGenres.Request>()))
                .ReturnsAsync(([genre], 1));

            var handler = new SearchGenres.Handler(repository.Object);
            var result = await handler.SearchGenresAsync(new SearchGenres.Request());

            var details = result.Value!.Genres.Single();
            Assert.AreEqual(genre.Id, details.Id);
            Assert.AreEqual(genre.Name, details.Name);
        }

        [TestMethod]
        [DataRow(0, 20, 0)]
        [DataRow(1, 20, 1)]
        [DataRow(20, 20, 1)]
        [DataRow(21, 20, 2)]
        [DataRow(100, 33, 4)]
        public async Task SearchGenresAsync_ComputesTotalPagesCorrectly(int totalCount, int pageSize, int expectedTotalPages)
        {
            var repository = new Mock<SearchGenres.IRepository>();
            repository.Setup(r => r.SearchGenresAsync(It.IsAny<SearchGenres.Request>()))
                .ReturnsAsync(([], totalCount));

            var handler = new SearchGenres.Handler(repository.Object);
            var result = await handler.SearchGenresAsync(new SearchGenres.Request { PageSize = pageSize });

            Assert.AreEqual(expectedTotalPages, result.Value!.TotalPages);
        }

        [TestMethod]
        public async Task SearchGenresAsync_EchoesTotalCountAndRequestPaging()
        {
            var repository = new Mock<SearchGenres.IRepository>();
            repository.Setup(r => r.SearchGenresAsync(It.IsAny<SearchGenres.Request>()))
                .ReturnsAsync(([], 42));

            var handler = new SearchGenres.Handler(repository.Object);
            var request = new SearchGenres.Request { PageNumber = 3, PageSize = 10 };
            var result = await handler.SearchGenresAsync(request);

            Assert.AreEqual(42, result.Value!.TotalCount);
            Assert.AreEqual(3, result.Value!.PageNumber);
            Assert.AreEqual(10, result.Value!.PageSize);
        }

        [TestMethod]
        public async Task SearchGenresAsync_WhenNoGenresFound_ReturnsEmptyGenresListWithZeroTotalPages()
        {
            var repository = new Mock<SearchGenres.IRepository>();
            repository.Setup(r => r.SearchGenresAsync(It.IsAny<SearchGenres.Request>()))
                .ReturnsAsync(([], 0));

            var handler = new SearchGenres.Handler(repository.Object);
            var result = await handler.SearchGenresAsync(new SearchGenres.Request());

            Assert.AreEqual(0, result.Value!.Genres.Count);
            Assert.AreEqual(0, result.Value!.TotalPages);
        }

        [TestMethod]
        public async Task SearchGenresAsync_PassesRequestThroughToRepository()
        {
            var repository = new Mock<SearchGenres.IRepository>();
            repository.Setup(r => r.SearchGenresAsync(It.IsAny<SearchGenres.Request>()))
                .ReturnsAsync(([], 0));

            var handler = new SearchGenres.Handler(repository.Object);
            var request = new SearchGenres.Request { SearchTerm = "action" };
            await handler.SearchGenresAsync(request);

            repository.Verify(r => r.SearchGenresAsync(request), Times.Once);
        }
    }

    [TestClass]
    public class SearchGenresControllerTests
    {
        private static SearchGenresController CreateController(SearchGenres.IHandler handler, IValidator<SearchGenres.Request> validator)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
            services.AddSingleton(Options.Create(new ApiBehaviorOptions()));
            var provider = services.BuildServiceProvider();

            return new SearchGenresController(handler, validator)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { RequestServices = provider }
                }
            };
        }

        [TestMethod]
        public async Task SearchGenresAsync_WhenValidationFails_ReturnsValidationProblemAndDoesNotCallHandler()
        {
            var handler = new Mock<SearchGenres.IHandler>();
            var validator = new Mock<IValidator<SearchGenres.Request>>();
            validator.Setup(v => v.ValidateAsync(It.IsAny<SearchGenres.Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult([new ValidationFailure("PageNumber", "must be greater than 0")]));

            var controller = CreateController(handler.Object, validator.Object);
            var result = await controller.SearchGenresAsync(new SearchGenres.Request());

            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(400, objectResult.StatusCode);
            handler.Verify(h => h.SearchGenresAsync(It.IsAny<SearchGenres.Request>()), Times.Never);
        }

        [TestMethod]
        public async Task SearchGenresAsync_WhenValidationSucceeds_ReturnsHandlerResultAndCallsHandlerOnce()
        {
            var expectedResponse = new SearchGenres.Response { TotalCount = 5 };
            var handler = new Mock<SearchGenres.IHandler>();
            var request = new SearchGenres.Request();
            handler.Setup(h => h.SearchGenresAsync(request)).ReturnsAsync(expectedResponse);

            var validator = new Mock<IValidator<SearchGenres.Request>>();
            validator.Setup(v => v.ValidateAsync(It.IsAny<SearchGenres.Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var controller = CreateController(handler.Object, validator.Object);
            var result = await controller.SearchGenresAsync(request);

            Assert.AreSame(expectedResponse, result.Value);
            handler.Verify(h => h.SearchGenresAsync(request), Times.Once);
        }
    }
}
