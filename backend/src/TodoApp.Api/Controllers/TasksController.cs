using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.Dtos;
using TodoApp.Application.Services;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private const string ActorHeaderName = "X-User";
    private readonly ITodoService _todoService;

    public TasksController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TodoItemDto>>> ListAsync(
        [FromQuery] TodoQueryDto query,
        CancellationToken cancellationToken)
    {
        var tasks = await _todoService.ListAsync(query, cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}", Name = "GetTaskById")]
    public async Task<ActionResult<TodoItemDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _todoService.GetByIdAsync(id, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TodoItemDto>> CreateAsync(
        [FromBody] CreateTodoItemDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdTask = await _todoService.CreateAsync(request, ResolveActor(), cancellationToken);
            return CreatedAtRoute("GetTaskById", new { id = createdTask.Id }, createdTask);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to create task.",
                Detail = exception.Message
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to create task.",
                Detail = exception.Message
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TodoItemDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateTodoItemDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedTask = await _todoService.UpdateAsync(id, request, ResolveActor(), cancellationToken);
            return updatedTask is null ? NotFound() : Ok(updatedTask);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to update task.",
                Detail = exception.Message
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to update task.",
                Detail = exception.Message
            });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _todoService.DeleteAsync(id, ResolveActor(), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private string ResolveActor()
    {
        if (Request.Headers.TryGetValue(ActorHeaderName, out var actorHeader) &&
            !string.IsNullOrWhiteSpace(actorHeader.ToString()))
        {
            return actorHeader.ToString().Trim();
        }

        return "anonymous";
    }
}
