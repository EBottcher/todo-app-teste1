using TodoApp.Domain.Entities;

namespace TodoApp.Application.Repositories;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLogEntry entry, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ActivityLogEntry>> ListAsync(int limit, CancellationToken cancellationToken = default);
}
