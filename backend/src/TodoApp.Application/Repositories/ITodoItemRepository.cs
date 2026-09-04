using TodoApp.Domain.Entities;

namespace TodoApp.Application.Repositories;

public interface ITodoItemRepository
{
    Task<IReadOnlyCollection<TodoItem>> ListAsync(CancellationToken cancellationToken = default);

    Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(TodoItem item, CancellationToken cancellationToken = default);

    Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
