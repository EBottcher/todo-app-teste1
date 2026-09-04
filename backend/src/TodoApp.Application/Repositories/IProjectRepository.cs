using TodoApp.Domain.Entities;

namespace TodoApp.Application.Repositories;

public interface IProjectRepository
{
    Task<IReadOnlyCollection<ProjectList>> ListAsync(CancellationToken cancellationToken = default);

    Task<ProjectList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(ProjectList project, CancellationToken cancellationToken = default);

    Task UpdateAsync(ProjectList project, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
