using TodoApp.Application.Dtos;

namespace TodoApp.Application.Services;

public interface ITodoService
{
    Task<IReadOnlyCollection<TodoItemDto>> ListAsync(TodoQueryDto query, CancellationToken cancellationToken = default);

    Task<TodoItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TodoItemDto> CreateAsync(CreateTodoItemDto request, string actor, CancellationToken cancellationToken = default);

    Task<TodoItemDto?> UpdateAsync(Guid id, UpdateTodoItemDto request, string actor, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, string actor, CancellationToken cancellationToken = default);
}
