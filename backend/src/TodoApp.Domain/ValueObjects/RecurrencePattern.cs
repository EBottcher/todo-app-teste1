using TodoApp.Domain.Enums;

namespace TodoApp.Domain.ValueObjects;

public sealed class RecurrencePattern
{
    public RecurrencePattern(RecurrenceFrequency frequency, int interval)
    {
        if (frequency == RecurrenceFrequency.None)
        {
            throw new ArgumentException("Frequency cannot be None when recurrence is configured.", nameof(frequency));
        }

        if (interval < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than zero.");
        }

        Frequency = frequency;
        Interval = interval;
    }

    public RecurrenceFrequency Frequency { get; }

    public int Interval { get; }
}
