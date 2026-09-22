using TodoApp.Application.Dtos;
using TodoApp.Application.Services;

namespace TodoApp.Api.Tests;

internal sealed class TestActivityService : IActivityService
{
    public Func<int, CancellationToken, Task<IReadOnlyCollection<ActivityLogDto>>> ListHandler { get; set; } =
        (_, _) => Task.FromResult<IReadOnlyCollection<ActivityLogDto>>([]);

    public Task<IReadOnlyCollection<ActivityLogDto>> ListAsync(
        int limit,
        CancellationToken cancellationToken = default) =>
        ListHandler(limit, cancellationToken);
}

internal sealed class TestProjectService : IProjectService
{
    public Func<CancellationToken, Task<IReadOnlyCollection<ProjectDto>>> ListHandler { get; set; } =
        _ => Task.FromResult<IReadOnlyCollection<ProjectDto>>([]);

    public Func<Guid, CancellationToken, Task<ProjectDto?>> GetByIdHandler { get; set; } =
        (_, _) => Task.FromResult<ProjectDto?>(null);

    public Func<CreateProjectDto, string, CancellationToken, Task<ProjectDto>> CreateHandler { get; set; } =
        (_, _, _) => throw new NotImplementedException();

    public Func<Guid, UpdateProjectDto, string, CancellationToken, Task<ProjectDto?>> UpdateHandler { get; set; } =
        (_, _, _, _) => Task.FromResult<ProjectDto?>(null);

    public Func<Guid, string, CancellationToken, Task<bool>> DeleteHandler { get; set; } =
        (_, _, _) => Task.FromResult(false);

    public Task<IReadOnlyCollection<ProjectDto>> ListAsync(CancellationToken cancellationToken = default) =>
        ListHandler(cancellationToken);

    public Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetByIdHandler(id, cancellationToken);

    public Task<ProjectDto> CreateAsync(
        CreateProjectDto request,
        string actor,
        CancellationToken cancellationToken = default) =>
        CreateHandler(request, actor, cancellationToken);

    public Task<ProjectDto?> UpdateAsync(
        Guid id,
        UpdateProjectDto request,
        string actor,
        CancellationToken cancellationToken = default) =>
        UpdateHandler(id, request, actor, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, string actor, CancellationToken cancellationToken = default) =>
        DeleteHandler(id, actor, cancellationToken);
}

internal sealed class TestTodoService : ITodoService
{
    public Func<TodoQueryDto, CancellationToken, Task<IReadOnlyCollection<TodoItemDto>>> ListHandler { get; set; } =
        (_, _) => Task.FromResult<IReadOnlyCollection<TodoItemDto>>([]);

    public Func<Guid, CancellationToken, Task<TodoItemDto?>> GetByIdHandler { get; set; } =
        (_, _) => Task.FromResult<TodoItemDto?>(null);

    public Func<CreateTodoItemDto, string, CancellationToken, Task<TodoItemDto>> CreateHandler { get; set; } =
        (_, _, _) => throw new NotImplementedException();

    public Func<Guid, UpdateTodoItemDto, string, CancellationToken, Task<TodoItemDto?>> UpdateHandler { get; set; } =
        (_, _, _, _) => Task.FromResult<TodoItemDto?>(null);

    public Func<Guid, string, CancellationToken, Task<bool>> DeleteHandler { get; set; } =
        (_, _, _) => Task.FromResult(false);

    public Task<IReadOnlyCollection<TodoItemDto>> ListAsync(
        TodoQueryDto query,
        CancellationToken cancellationToken = default) =>
        ListHandler(query, cancellationToken);

    public Task<TodoItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetByIdHandler(id, cancellationToken);

    public Task<TodoItemDto> CreateAsync(
        CreateTodoItemDto request,
        string actor,
        CancellationToken cancellationToken = default) =>
        CreateHandler(request, actor, cancellationToken);

    public Task<TodoItemDto?> UpdateAsync(
        Guid id,
        UpdateTodoItemDto request,
        string actor,
        CancellationToken cancellationToken = default) =>
        UpdateHandler(id, request, actor, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, string actor, CancellationToken cancellationToken = default) =>
        DeleteHandler(id, actor, cancellationToken);
}
