using TodoApp.Application.Dtos;

namespace TodoApp.Application.Services;

public interface IProjectService
{
    Task<IReadOnlyCollection<ProjectDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProjectDto> CreateAsync(CreateProjectDto request, string actor, CancellationToken cancellationToken = default);

    Task<ProjectDto?> UpdateAsync(Guid id, UpdateProjectDto request, string actor, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, string actor, CancellationToken cancellationToken = default);
}
