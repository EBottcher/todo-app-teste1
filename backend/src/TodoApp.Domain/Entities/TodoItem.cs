using TodoApp.Domain.Enums;
using TodoApp.Domain.ValueObjects;

namespace TodoApp.Domain.Entities;

public sealed class TodoItem
{
    private readonly List<string> _tags = [];
    private readonly List<DateTimeOffset> _remindersUtc = [];

    private TodoItem()
    {
    }

    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public TodoStatus Status { get; private set; }

    public TodoPriority Priority { get; private set; }

    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    public DateTimeOffset? DueDateUtc { get; private set; }

    public IReadOnlyCollection<DateTimeOffset> RemindersUtc => _remindersUtc.AsReadOnly();

    public RecurrencePattern? Recurrence { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;

    public string UpdatedBy { get; private set; } = string.Empty;

    public static TodoItem Create(
        Guid id,
        Guid projectId,
        string title,
        string? description,
        TodoStatus status,
        TodoPriority priority,
        IEnumerable<string>? tags,
        DateTimeOffset? dueDateUtc,
        IEnumerable<DateTimeOffset>? remindersUtc,
        RecurrencePattern? recurrence,
        string actor,
        DateTimeOffset nowUtc)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project identifier is required.", nameof(projectId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title is required.", nameof(title));
        }

        var entity = new TodoItem
        {
            Id = id,
            ProjectId = projectId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Status = status,
            Priority = priority,
            DueDateUtc = dueDateUtc,
            Recurrence = recurrence,
            CreatedBy = actor,
            UpdatedBy = actor,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };

        entity.SetTags(tags);
        entity.SetReminders(remindersUtc);
        return entity;
    }

    public void Update(
        Guid projectId,
        string title,
        string? description,
        TodoStatus status,
        TodoPriority priority,
        IEnumerable<string>? tags,
        DateTimeOffset? dueDateUtc,
        IEnumerable<DateTimeOffset>? remindersUtc,
        RecurrencePattern? recurrence,
        string actor,
        DateTimeOffset nowUtc)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project identifier is required.", nameof(projectId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title is required.", nameof(title));
        }

        ProjectId = projectId;
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Status = status;
        Priority = priority;
        DueDateUtc = dueDateUtc;
        Recurrence = recurrence;
        UpdatedBy = actor;
        UpdatedAtUtc = nowUtc;

        SetTags(tags);
        SetReminders(remindersUtc);
    }

    private void SetTags(IEnumerable<string>? tags)
    {
        _tags.Clear();
        if (tags is null)
        {
            return;
        }

        foreach (var tag in tags
                     .Select(tag => tag.Trim())
                     .Where(tag => !string.IsNullOrWhiteSpace(tag))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _tags.Add(tag);
        }
    }

    private void SetReminders(IEnumerable<DateTimeOffset>? remindersUtc)
    {
        _remindersUtc.Clear();
        if (remindersUtc is null)
        {
            return;
        }

        foreach (var reminderUtc in remindersUtc
                     .Distinct()
                     .OrderBy(reminderUtc => reminderUtc))
        {
            _remindersUtc.Add(reminderUtc);
        }
    }
}
