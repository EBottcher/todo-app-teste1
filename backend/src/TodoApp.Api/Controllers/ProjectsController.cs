using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.Dtos;
using TodoApp.Application.Services;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/projects")]
public sealed class ProjectsController : ControllerBase
{
    private const string ActorHeaderName = "X-User";
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProjectDto>>> ListAsync(CancellationToken cancellationToken)
    {
        var projects = await _projectService.ListAsync(cancellationToken);
        return Ok(projects);
    }

    [HttpGet("{id:guid}", Name = "GetProjectById")]
    public async Task<ActionResult<ProjectDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetByIdAsync(id, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateAsync(
        [FromBody] CreateProjectDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdProject = await _projectService.CreateAsync(request, ResolveActor(), cancellationToken);
            return CreatedAtRoute("GetProjectById", new { id = createdProject.Id }, createdProject);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to create project.",
                Detail = exception.Message
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> UpdateAsync(
        Guid id,
        [FromBody] UpdateProjectDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updatedProject = await _projectService.UpdateAsync(id, request, ResolveActor(), cancellationToken);
            return updatedProject is null ? NotFound() : Ok(updatedProject);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to update project.",
                Detail = exception.Message
            });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _projectService.DeleteAsync(id, ResolveActor(), cancellationToken);
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
