namespace Planner.Core.Models;

public sealed class TaskDraft
{
    public long? Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long AssignedToUserId { get; set; }
    public DateTime? StartDate { get; set; }
    public long DurationSeconds { get; set; } = 3600;
    public bool IsAllDay { get; set; }
    public string? CronSchedule { get; set; }
    public bool ShiftWeekendToWeekday { get; set; }
    public string Status { get; set; } = TaskStatuses.Pending;
    public long? ReminderOffsetSeconds { get; set; }
}
