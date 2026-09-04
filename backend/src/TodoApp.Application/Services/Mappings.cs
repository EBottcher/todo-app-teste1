using TodoApp.Application.Dtos;
using TodoApp.Domain.Entities;
using TodoApp.Domain.ValueObjects;

namespace TodoApp.Application.Services;

internal static class Mappings
{
    public static TodoItemDto ToDto(this TodoItem entity) =>
        new()
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status,
            Priority = entity.Priority,
            Tags = entity.Tags.ToList(),
            DueDateUtc = entity.DueDateUtc,
            RemindersUtc = entity.RemindersUtc.ToList(),
            Recurrence = entity.Recurrence?.ToDto(),
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };

    public static ProjectDto ToDto(this ProjectList entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            ColorHex = entity.ColorHex,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy
        };

    public static ActivityLogDto ToDto(this ActivityLogEntry entity) =>
        new()
        {
            Id = entity.Id,
            EntityType = entity.EntityType,
            EntityId = entity.EntityId,
            Action = entity.Action,
            ChangedBy = entity.ChangedBy,
            ChangedAtUtc = entity.ChangedAtUtc,
            Details = entity.Details
        };

    private static RecurrencePatternDto ToDto(this RecurrencePattern entity) =>
        new()
        {
            Frequency = entity.Frequency,
            Interval = entity.Interval
        };
}
