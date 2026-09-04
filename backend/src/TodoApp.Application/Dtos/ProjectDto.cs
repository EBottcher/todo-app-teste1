namespace TodoApp.Application.Dtos;

public sealed class ProjectDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? ColorHex { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }

    public string CreatedBy { get; init; } = string.Empty;

    public string UpdatedBy { get; init; } = string.Empty;
}
