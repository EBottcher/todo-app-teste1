using TodoApp.Domain.Enums;

namespace TodoApp.Application.Dtos;

public sealed class RecurrencePatternDto
{
    public RecurrenceFrequency Frequency { get; init; }

    public int Interval { get; init; }
}
