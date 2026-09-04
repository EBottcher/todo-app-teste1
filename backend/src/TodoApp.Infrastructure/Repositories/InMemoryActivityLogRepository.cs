using System.Collections.Concurrent;
using TodoApp.Application.Repositories;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Repositories;

public sealed class InMemoryActivityLogRepository : IActivityLogRepository
{
    private readonly ConcurrentQueue<ActivityLogEntry> _entries = new();

    public Task AddAsync(ActivityLogEntry entry, CancellationToken cancellationToken = default)
    {
        _entries.Enqueue(entry);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<ActivityLogEntry>> ListAsync(int limit, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<ActivityLogEntry> result = _entries
            .OrderByDescending(entry => entry.ChangedAtUtc)
            .Take(limit)
            .ToList();

        return Task.FromResult(result);
    }
}
