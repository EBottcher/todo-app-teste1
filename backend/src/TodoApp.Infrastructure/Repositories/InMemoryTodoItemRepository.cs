using System.Collections.Concurrent;
using TodoApp.Application.Repositories;
using TodoApp.Domain.Entities;

namespace TodoApp.Infrastructure.Repositories;

public sealed class InMemoryTodoItemRepository : ITodoItemRepository
{
    private readonly ConcurrentDictionary<Guid, TodoItem> _items = new();

    public Task<IReadOnlyCollection<TodoItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<TodoItem> result = _items.Values.ToList();
        return Task.FromResult(result);
    }

    public Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _items.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    public Task AddAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        if (!_items.TryAdd(item.Id, item))
        {
            throw new InvalidOperationException($"A task with id '{item.Id}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        _items[item.Id] = item;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = _items.TryRemove(id, out _);
        return Task.FromResult(deleted);
    }
}
