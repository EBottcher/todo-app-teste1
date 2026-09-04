using TodoApp.Application.Dtos;

namespace TodoApp.Application.Services;

public interface IActivityService
{
    Task<IReadOnlyCollection<ActivityLogDto>> ListAsync(int limit, CancellationToken cancellationToken = default);
}
