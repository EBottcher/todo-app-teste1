using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.Controllers;
using TodoApp.Application.Dtos;

namespace TodoApp.Api.Tests;

public sealed class ProjectsControllerTests
{
    [Fact]
    public async Task ListAsync_ReturnsProjects()
    {
        IReadOnlyCollection<ProjectDto> projects = [new() { Name = "Work" }];
        var service = new TestProjectService { ListHandler = _ => Task.FromResult(projects) };

        var result = await new ProjectsController(service).ListAsync(default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(projects, ok.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProjectExists_ReturnsProject()
    {
        var project = new ProjectDto { Id = Guid.NewGuid(), Name = "Work" };
        var service = new TestProjectService
        {
            GetByIdHandler = (id, _) => Task.FromResult<ProjectDto?>(id == project.Id ? project : null)
        };

        var result = await new ProjectsController(service).GetByIdAsync(project.Id, default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(project, ok.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProjectDoesNotExist_ReturnsNotFound()
    {
        var result = await new ProjectsController(new TestProjectService()).GetByIdAsync(Guid.NewGuid(), default);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedProjectAndUsesTrimmedActor()
    {
        var request = new CreateProjectDto { Name = "Work" };
        var project = new ProjectDto { Id = Guid.NewGuid(), Name = request.Name };
        var service = new TestProjectService
        {
            CreateHandler = (actualRequest, actor, _) =>
            {
                Assert.Same(request, actualRequest);
                Assert.Equal("Taylor", actor);
                return Task.FromResult(project);
            }
        };
        var controller = CreateController(service, "  Taylor  ");

        var result = await controller.CreateAsync(request, default);

        var created = Assert.IsType<CreatedAtRouteResult>(result.Result);
        Assert.Equal("GetProjectById", created.RouteName);
        Assert.Equal(project.Id, created.RouteValues!["id"]);
        Assert.Same(project, created.Value);
    }

    [Fact]
    public async Task CreateAsync_WhenValidationFails_ReturnsProblemDetails()
    {
        var service = new TestProjectService
        {
            CreateHandler = (_, _, _) => throw new ArgumentException("Name is required.")
        };

        var result = await CreateController(service).CreateAsync(new CreateProjectDto(), default);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Unable to create project.", problem.Title);
        Assert.Equal("Name is required.", problem.Detail);
    }

    [Fact]
    public async Task UpdateAsync_WhenProjectExists_ReturnsProjectAndUsesAnonymousActor()
    {
        var id = Guid.NewGuid();
        var request = new UpdateProjectDto { Name = "Personal" };
        var project = new ProjectDto { Id = id, Name = request.Name };
        var service = new TestProjectService
        {
            UpdateHandler = (actualId, actualRequest, actor, _) =>
            {
                Assert.Equal(id, actualId);
                Assert.Same(request, actualRequest);
                Assert.Equal("anonymous", actor);
                return Task.FromResult<ProjectDto?>(project);
            }
        };

        var result = await CreateController(service, "   ").UpdateAsync(id, request, default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(project, ok.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenProjectDoesNotExist_ReturnsNotFound()
    {
        var result = await CreateController(new TestProjectService())
            .UpdateAsync(Guid.NewGuid(), new UpdateProjectDto { Name = "Missing" }, default);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidationFails_ReturnsProblemDetails()
    {
        var service = new TestProjectService
        {
            UpdateHandler = (_, _, _, _) => throw new ArgumentException("Name is required.")
        };

        var result = await CreateController(service)
            .UpdateAsync(Guid.NewGuid(), new UpdateProjectDto(), default);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Unable to update project.", problem.Title);
        Assert.Equal("Name is required.", problem.Detail);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteAsync_ReturnsResultForServiceOutcome(bool deleted)
    {
        var service = new TestProjectService
        {
            DeleteHandler = (_, actor, _) =>
            {
                Assert.Equal("Morgan", actor);
                return Task.FromResult(deleted);
            }
        };

        var result = await CreateController(service, "Morgan").DeleteAsync(Guid.NewGuid(), default);

        if (deleted)
        {
            Assert.IsType<NoContentResult>(result);
        }
        else
        {
            Assert.IsType<NotFoundResult>(result);
        }
    }

    private static ProjectsController CreateController(TestProjectService service, string? actor = null)
    {
        var context = new DefaultHttpContext();
        if (actor is not null)
        {
            context.Request.Headers["X-User"] = actor;
        }

        return new ProjectsController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }
}
