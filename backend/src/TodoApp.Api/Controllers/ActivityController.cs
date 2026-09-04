using Microsoft.AspNetCore.Mvc;
using TodoApp.Application.Dtos;
using TodoApp.Application.Services;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/activity")]
public sealed class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ActivityLogDto>>> ListAsync(
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var activity = await _activityService.ListAsync(limit, cancellationToken);
        return Ok(activity);
    }
}
