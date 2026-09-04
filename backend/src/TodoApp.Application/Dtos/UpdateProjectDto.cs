using System.ComponentModel.DataAnnotations;

namespace TodoApp.Application.Dtos;

public sealed class UpdateProjectDto
{
    [Required]
    [MaxLength(120)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; init; }

    [MaxLength(20)]
    public string? ColorHex { get; init; }
}
