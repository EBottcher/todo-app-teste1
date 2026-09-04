namespace TodoApp.Domain.Entities;

public sealed class ProjectList
{
    private ProjectList()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? ColorHex { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;

    public string UpdatedBy { get; private set; } = string.Empty;

    public static ProjectList Create(
        Guid id,
        string name,
        string? description,
        string? colorHex,
        string actor,
        DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name is required.", nameof(name));
        }

        return new ProjectList
        {
            Id = id,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            ColorHex = string.IsNullOrWhiteSpace(colorHex) ? null : colorHex.Trim(),
            CreatedBy = actor,
            UpdatedBy = actor,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };
    }

    public void Update(string name, string? description, string? colorHex, string actor, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name is required.", nameof(name));
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ColorHex = string.IsNullOrWhiteSpace(colorHex) ? null : colorHex.Trim();
        UpdatedBy = actor;
        UpdatedAtUtc = nowUtc;
    }
}
