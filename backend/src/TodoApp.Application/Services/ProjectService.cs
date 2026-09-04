using TodoApp.Application.Dtos;
using TodoApp.Application.Repositories;
using TodoApp.Domain.Entities;

namespace TodoApp.Application.Services;

public sealed class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public ProjectService(IProjectRepository projectRepository, IActivityLogRepository activityLogRepository)
    {
        _projectRepository = projectRepository;
        _activityLogRepository = activityLogRepository;
    }

    public async Task<IReadOnlyCollection<ProjectDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return (await _projectRepository.ListAsync(cancellationToken))
            .OrderBy(project => project.Name)
            .Select(project => project.ToDto())
            .ToList();
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        return project?.ToDto();
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto request, string actor, CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var project = ProjectList.Create(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            request.ColorHex,
            actor,
            nowUtc);

        await _projectRepository.AddAsync(project, cancellationToken);
        await _activityLogRepository.AddAsync(
            ActivityLogEntry.Create(
                "Project",
                project.Id,
                "Created",
                actor,
                nowUtc,
                $"Created project '{project.Name}'."),
            cancellationToken);

        return project.ToDto();
    }

    public async Task<ProjectDto?> UpdateAsync(Guid id, UpdateProjectDto request, string actor, CancellationToken cancellationToken = default)
    {
        var existingProject = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (existingProject is null)
        {
            return null;
        }

        var nowUtc = DateTimeOffset.UtcNow;
        existingProject.Update(request.Name, request.Description, request.ColorHex, actor, nowUtc);
        await _projectRepository.UpdateAsync(existingProject, cancellationToken);
        await _activityLogRepository.AddAsync(
            ActivityLogEntry.Create(
                "Project",
                existingProject.Id,
                "Updated",
                actor,
                nowUtc,
                $"Updated project '{existingProject.Name}'."),
            cancellationToken);

        return existingProject.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, string actor, CancellationToken cancellationToken = default)
    {
        var existingProject = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (existingProject is null)
        {
            return false;
        }

        var deleted = await _projectRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return false;
        }

        await _activityLogRepository.AddAsync(
            ActivityLogEntry.Create(
                "Project",
                existingProject.Id,
                "Deleted",
                actor,
                DateTimeOffset.UtcNow,
                $"Deleted project '{existingProject.Name}'."),
            cancellationToken);

        return true;
    }
}
