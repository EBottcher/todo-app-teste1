using System.ComponentModel.DataAnnotations;
using TodoApp.Domain.Enums;

namespace TodoApp.Application.Dtos;

public sealed class RecurrencePatternInputDto
{
    [Required]
    public RecurrenceFrequency Frequency { get; init; } = RecurrenceFrequency.None;

    [Range(1, 3650)]
    public int Interval { get; init; } = 1;
}
