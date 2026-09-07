namespace Planner.Core.Models;

public sealed class Reminder
{
    public long Id { get; set; }
    public long TaskId { get; set; }
    public long OffsetSeconds { get; set; }
    public bool IsTriggered { get; set; }
}

public sealed class DueReminder
{
    public long ReminderId { get; init; }
    public long TaskId { get; init; }
    public string TaskTitle { get; init; } = string.Empty;
    public DateTime TaskStart { get; init; }
    public long OffsetSeconds { get; init; }
    public DateTime TriggerAt => TaskStart.AddSeconds(-OffsetSeconds);
}
