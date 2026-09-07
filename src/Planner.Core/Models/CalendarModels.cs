namespace Planner.Core.Models;

public enum CalendarViewMode
{
    Day,
    WorkWeek,
    Month
}

public sealed record ReminderUnit(string Name, long Multiplier);

public sealed record ReminderDisplay(long Value, ReminderUnit Unit);
