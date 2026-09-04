namespace TodoApp.Domain.Entities;

public sealed class ActivityLogEntry
{
    private ActivityLogEntry()
    {
    }

    public Guid Id { get; private set; }

    public string EntityType { get; private set; } = string.Empty;

    public Guid EntityId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string ChangedBy { get; private set; } = string.Empty;

    public DateTimeOffset ChangedAtUtc { get; private set; }

    public string Details { get; private set; } = string.Empty;

    public static ActivityLogEntry Create(
        string entityType,
        Guid entityId,
        string action,
        string changedBy,
        DateTimeOffset changedAtUtc,
        string details)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            throw new ArgumentException("Entity type is required.", nameof(entityType));
        }

        if (entityId == Guid.Empty)
        {
            throw new ArgumentException("Entity identifier is required.", nameof(entityId));
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Action is required.", nameof(action));
        }

        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new ArgumentException("Changed by is required.", nameof(changedBy));
        }

        return new ActivityLogEntry
        {
            Id = Guid.NewGuid(),
            EntityType = entityType.Trim(),
            EntityId = entityId,
            Action = action.Trim(),
            ChangedBy = changedBy.Trim(),
            ChangedAtUtc = changedAtUtc,
            Details = details.Trim()
        };
    }
}
