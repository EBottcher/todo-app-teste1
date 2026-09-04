using System.Collections.Concurrent;
using TodoApp.Application.Repositories;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Repositories;

public sealed class InMemoryProjectRepository : IProjectRepository
{
    private static readonly Guid InboxProjectId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly ConcurrentDictionary<Guid, ProjectList> _projects = new();

    public InMemoryProjectRepository()
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var inboxProject = ProjectList.Create(
            InboxProjectId,
            "Inbox",
            "Default list for uncategorized tasks.",
            "#2563eb",
            "system",
            nowUtc);

        _projects[inboxProject.Id] = inboxProject;
    }

    public Task<IReadOnlyCollection<ProjectList>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<ProjectList> result = _projects.Values.ToList();
        return Task.FromResult(result);
    }

    public Task<ProjectList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _projects.TryGetValue(id, out var project);
        return Task.FromResult(project);
    }

    public Task AddAsync(ProjectList project, CancellationToken cancellationToken = default)
    {
        if (!_projects.TryAdd(project.Id, project))
        {
            throw new InvalidOperationException($"A project with id '{project.Id}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(ProjectList project, CancellationToken cancellationToken = default)
    {
        _projects[project.Id] = project;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = _projects.TryRemove(id, out _);
        return Task.FromResult(deleted);
    }
}
