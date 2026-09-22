using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.Controllers;
using TodoApp.Application.Dtos;

namespace TodoApp.Api.Tests;

public sealed class TasksControllerTests
{
    [Fact]
    public async Task ListAsync_ReturnsTasksAndForwardsQuery()
    {
        var query = new TodoQueryDto { Tag = "important" };
        IReadOnlyCollection<TodoItemDto> tasks = [new() { Title = "Ship release" }];
        var service = new TestTodoService
        {
            ListHandler = (actualQuery, _) =>
            {
                Assert.Same(query, actualQuery);
                return Task.FromResult(tasks);
            }
        };

        var result = await new TasksController(service).ListAsync(query, default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(tasks, ok.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskExists_ReturnsTask()
    {
        var task = new TodoItemDto { Id = Guid.NewGuid(), Title = "Ship release" };
        var service = new TestTodoService
        {
            GetByIdHandler = (id, _) => Task.FromResult<TodoItemDto?>(id == task.Id ? task : null)
        };

        var result = await new TasksController(service).GetByIdAsync(task.Id, default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(task, ok.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        var result = await new TasksController(new TestTodoService()).GetByIdAsync(Guid.NewGuid(), default);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedTaskAndUsesTrimmedActor()
    {
        var request = new CreateTodoItemDto { Title = "Ship release" };
        var task = new TodoItemDto { Id = Guid.NewGuid(), Title = request.Title };
        var service = new TestTodoService
        {
            CreateHandler = (actualRequest, actor, _) =>
            {
                Assert.Same(request, actualRequest);
                Assert.Equal("Taylor", actor);
                return Task.FromResult(task);
            }
        };

        var result = await CreateController(service, " Taylor ").CreateAsync(request, default);

        var created = Assert.IsType<CreatedAtRouteResult>(result.Result);
        Assert.Equal("GetTaskById", created.RouteName);
        Assert.Equal(task.Id, created.RouteValues!["id"]);
        Assert.Same(task, created.Value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateAsync_WhenServiceRejectsRequest_ReturnsProblemDetails(bool invalidOperation)
    {
        var exception = invalidOperation
            ? new InvalidOperationException("Project does not exist.")
            : new ArgumentException("Title is required.");
        var service = new TestTodoService
        {
            CreateHandler = (_, _, _) => Task.FromException<TodoItemDto>(exception)
        };

        var result = await CreateController(service).CreateAsync(new CreateTodoItemDto(), default);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Unable to create task.", problem.Title);
        Assert.Equal(exception.Message, problem.Detail);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskExists_ReturnsTaskAndUsesAnonymousActor()
    {
        var id = Guid.NewGuid();
        var request = new UpdateTodoItemDto { Title = "Updated" };
        var task = new TodoItemDto { Id = id, Title = request.Title };
        var service = new TestTodoService
        {
            UpdateHandler = (actualId, actualRequest, actor, _) =>
            {
                Assert.Equal(id, actualId);
                Assert.Same(request, actualRequest);
                Assert.Equal("anonymous", actor);
                return Task.FromResult<TodoItemDto?>(task);
            }
        };

        var result = await CreateController(service).UpdateAsync(id, request, default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(task, ok.Value);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        var result = await CreateController(new TestTodoService())
            .UpdateAsync(Guid.NewGuid(), new UpdateTodoItemDto { Title = "Missing" }, default);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAsync_WhenServiceRejectsRequest_ReturnsProblemDetails(bool invalidOperation)
    {
        var exception = invalidOperation
            ? new InvalidOperationException("Project does not exist.")
            : new ArgumentException("Title is required.");
        var service = new TestTodoService
        {
            UpdateHandler = (_, _, _, _) => Task.FromException<TodoItemDto?>(exception)
        };

        var result = await CreateController(service)
            .UpdateAsync(Guid.NewGuid(), new UpdateTodoItemDto(), default);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Unable to update task.", problem.Title);
        Assert.Equal(exception.Message, problem.Detail);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteAsync_ReturnsResultForServiceOutcome(bool deleted)
    {
        var service = new TestTodoService
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

    private static TasksController CreateController(TestTodoService service, string? actor = null)
    {
        var context = new DefaultHttpContext();
        if (actor is not null)
        {
            context.Request.Headers["X-User"] = actor;
        }

        return new TasksController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }
}
