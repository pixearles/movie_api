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

namespace Movies.API.Features.Actors.Search.v1
{
    [TestClass]
    public class SearchActorsValidatorTests
    {
        private readonly SearchActors.Validator _validator = new();

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public void PageNumber_WhenNotPositive_HasValidationError(int pageNumber)
        {
            var result = _validator.TestValidate(new SearchActors.Request { PageNumber = pageNumber });

            result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        }

        [TestMethod]
        public void PageNumber_WhenPositive_HasNoValidationError()
        {
            var result = _validator.TestValidate(new SearchActors.Request { PageNumber = 1 });

            result.ShouldNotHaveValidationErrorFor(x => x.PageNumber);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(101)]
        public void PageSize_WhenOutsideInclusiveRange_HasValidationError(int pageSize)
        {
            var result = _validator.TestValidate(new SearchActors.Request { PageSize = pageSize });

            result.ShouldHaveValidationErrorFor(x => x.PageSize);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(50)]
        [DataRow(100)]
        public void PageSize_WhenWithinInclusiveRange_HasNoValidationError(int pageSize)
        {
            var result = _validator.TestValidate(new SearchActors.Request { PageSize = pageSize });

            result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
        }
    }

    [TestClass]
    public class SearchActorsHandlerTests
    {
        private static Actor CreateActor(int id, string name) => new()
        {
            Id = id,
            Name = name
        };

        [TestMethod]
        public async Task SearchActorsAsync_MapsRepositoryActorsToActorSummaries()
        {
            var actor = CreateActor(1, "Alice");
            var repository = new Mock<SearchActors.IRepository>();
            repository.Setup(r => r.SearchActorsAsync(It.IsAny<SearchActors.Request>()))
                .ReturnsAsync(([actor], 1));

            var handler = new SearchActors.Handler(repository.Object);
            var result = await handler.SearchActorsAsync(new SearchActors.Request());

            var summary = result.Value!.Actors.Single();
            Assert.AreEqual(actor.Id, summary.Id);
            Assert.AreEqual(actor.Name, summary.Name);
        }

        [TestMethod]
        [DataRow(0, 20, 0)]
        [DataRow(1, 20, 1)]
        [DataRow(20, 20, 1)]
        [DataRow(21, 20, 2)]
        [DataRow(100, 33, 4)]
        public async Task SearchActorsAsync_ComputesTotalPagesCorrectly(int totalCount, int pageSize, int expectedTotalPages)
        {
            var repository = new Mock<SearchActors.IRepository>();
            repository.Setup(r => r.SearchActorsAsync(It.IsAny<SearchActors.Request>()))
                .ReturnsAsync(([], totalCount));

            var handler = new SearchActors.Handler(repository.Object);
            var result = await handler.SearchActorsAsync(new SearchActors.Request { PageSize = pageSize });

            Assert.AreEqual(expectedTotalPages, result.Value!.TotalPages);
        }

        [TestMethod]
        public async Task SearchActorsAsync_EchoesTotalCountAndRequestPaging()
        {
            var repository = new Mock<SearchActors.IRepository>();
            repository.Setup(r => r.SearchActorsAsync(It.IsAny<SearchActors.Request>()))
                .ReturnsAsync(([], 42));

            var handler = new SearchActors.Handler(repository.Object);
            var request = new SearchActors.Request { PageNumber = 3, PageSize = 10 };
            var result = await handler.SearchActorsAsync(request);

            Assert.AreEqual(42, result.Value!.TotalCount);
            Assert.AreEqual(3, result.Value!.PageNumber);
            Assert.AreEqual(10, result.Value!.PageSize);
        }

        [TestMethod]
        public async Task SearchActorsAsync_WhenNoActorsFound_ReturnsEmptyActorsListWithZeroTotalPages()
        {
            var repository = new Mock<SearchActors.IRepository>();
            repository.Setup(r => r.SearchActorsAsync(It.IsAny<SearchActors.Request>()))
                .ReturnsAsync(([], 0));

            var handler = new SearchActors.Handler(repository.Object);
            var result = await handler.SearchActorsAsync(new SearchActors.Request());

            Assert.AreEqual(0, result.Value!.Actors.Count);
            Assert.AreEqual(0, result.Value!.TotalPages);
        }

        [TestMethod]
        public async Task SearchActorsAsync_PassesRequestThroughToRepository()
        {
            var repository = new Mock<SearchActors.IRepository>();
            repository.Setup(r => r.SearchActorsAsync(It.IsAny<SearchActors.Request>()))
                .ReturnsAsync(([], 0));

            var handler = new SearchActors.Handler(repository.Object);
            var request = new SearchActors.Request { SearchTerm = "alice" };
            await handler.SearchActorsAsync(request);

            repository.Verify(r => r.SearchActorsAsync(request), Times.Once);
        }
    }

    [TestClass]
    public class SearchActorsControllerTests
    {
        private static SearchActorsController CreateController(SearchActors.IHandler handler, IValidator<SearchActors.Request> validator)
        {
            var services = new ServiceCollection();
            services.AddSingleton<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
            services.AddSingleton(Options.Create(new ApiBehaviorOptions()));
            var provider = services.BuildServiceProvider();

            return new SearchActorsController(handler, validator)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { RequestServices = provider }
                }
            };
        }

        [TestMethod]
        public async Task SearchActorsAsync_WhenValidationFails_ReturnsValidationProblemAndDoesNotCallHandler()
        {
            var handler = new Mock<SearchActors.IHandler>();
            var validator = new Mock<IValidator<SearchActors.Request>>();
            validator.Setup(v => v.ValidateAsync(It.IsAny<SearchActors.Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult([new ValidationFailure("PageNumber", "must be greater than 0")]));

            var controller = CreateController(handler.Object, validator.Object);
            var result = await controller.SearchActorsAsync(new SearchActors.Request());

            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(400, objectResult.StatusCode);
            handler.Verify(h => h.SearchActorsAsync(It.IsAny<SearchActors.Request>()), Times.Never);
        }

        [TestMethod]
        public async Task SearchActorsAsync_WhenValidationSucceeds_ReturnsHandlerResultAndCallsHandlerOnce()
        {
            var expectedResponse = new SearchActors.Response { TotalCount = 5 };
            var handler = new Mock<SearchActors.IHandler>();
            var request = new SearchActors.Request();
            handler.Setup(h => h.SearchActorsAsync(request)).ReturnsAsync(expectedResponse);

            var validator = new Mock<IValidator<SearchActors.Request>>();
            validator.Setup(v => v.ValidateAsync(It.IsAny<SearchActors.Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var controller = CreateController(handler.Object, validator.Object);
            var result = await controller.SearchActorsAsync(request);

            Assert.AreSame(expectedResponse, result.Value);
            handler.Verify(h => h.SearchActorsAsync(request), Times.Once);
        }
    }
}
