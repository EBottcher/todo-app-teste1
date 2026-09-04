using TodoApp.Application.Dtos;
using TodoApp.Application.Repositories;

namespace TodoApp.Application.Services;

public sealed class ActivityService : IActivityService
{
    private readonly IActivityLogRepository _activityLogRepository;

    public ActivityService(IActivityLogRepository activityLogRepository)
    {
        _activityLogRepository = activityLogRepository;
    }

    public async Task<IReadOnlyCollection<ActivityLogDto>> ListAsync(int limit, CancellationToken cancellationToken = default)
    {
        var boundedLimit = Math.Clamp(limit, 1, 500);
        return (await _activityLogRepository.ListAsync(boundedLimit, cancellationToken))
            .Select(entry => entry.ToDto())
            .ToList();
    }
}
