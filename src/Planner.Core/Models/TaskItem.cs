using Planner.Core.Services;

namespace Planner.Core.Models;

public sealed class TaskItem
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long CreatedByUserId { get; set; }
    public long AssignedToUserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long? DurationSeconds { get; set; }
    public bool IsAllDay { get; set; }
    public string? CronSchedule { get; set; }

    /// <summary>Переносить повторы, выпавшие на субботу или воскресенье, на ближайший будний день.</summary>
    public bool ShiftWeekendToWeekday { get; set; }
    public string Status { get; set; } = TaskStatuses.Pending;
    public long? SourceTaskId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string AssignedToDisplayName { get; set; } = string.Empty;
    public string CreatedByDisplayName { get; set; } = string.Empty;

    public bool HasTime => !IsAllDay;

    /// <summary>
    /// Просрочка — вычисляемое состояние. В БД оно не хранится отдельным статусом:
    /// для задач со временем сравнивается конец задачи, для задач без времени — дата.
    /// </summary>
    public bool IsOverdue
    {
        get
        {
            if (Status == TaskStatuses.Completed || StartDate is null)
                return false;

            if (IsAllDay)
                return StartDate.Value.Date < DateTime.Today;

            var dueAt = EndDate;
            if (dueAt is null || dueAt.Value <= StartDate.Value)
                dueAt = StartDate.Value + EffectiveDuration;

            return dueAt.Value < DateTime.Now;
        }
    }

    /// <summary>
    /// Состояние для визуального оформления календаря.
    /// Приоритет: выполнено -> просрочено -> в работе -> ожидает.
    /// </summary>
    public string VisualState =>
        Status == TaskStatuses.Completed ? TaskVisualStates.Completed :
        IsOverdue ? TaskVisualStates.Overdue :
        Status == TaskStatuses.InProgress ? TaskVisualStates.InProgress :
        TaskVisualStates.Pending;

    public string StatusDisplay => IsOverdue ? "Просрочено" : TaskStatuses.ToRussian(Status);

    /// <summary>Время задачи для списков и подсказок: «09:00–10:00» либо «Весь день».</summary>
    public string TimeDisplay
    {
        get
        {
            if (StartDate is null) return string.Empty;
            if (IsAllDay) return "Весь день";
            var start = StartDate.Value;
            return $"{start:HH:mm}–{start.Add(EffectiveDuration):HH:mm}";
        }
    }

    /// <summary>Человеко-читаемое расписание повторения вместо сырого Cron.</summary>
    public string ScheduleDisplay => CronDescriber.Describe(CronSchedule);

    public bool IsRecurringTemplate => SourceTaskId is null && !string.IsNullOrWhiteSpace(CronSchedule);

    /// <summary>Пояснение для дубликата просроченной задачи в текущем дне: «Просрочено с 05.09».</summary>
    public string OverdueOriginDisplay =>
        StartDate is null ? "Просрочено" : $"Просрочено с {StartDate.Value:dd.MM.yyyy}";
    public string AssigneeDisplay => string.IsNullOrWhiteSpace(AssignedToDisplayName) ? $"ID {AssignedToUserId}" : AssignedToDisplayName;
    public string CreatorDisplay => string.IsNullOrWhiteSpace(CreatedByDisplayName) ? $"ID {CreatedByUserId}" : CreatedByDisplayName;

    public TimeSpan EffectiveDuration
    {
        get
        {
            if (DurationSeconds is > 0) return TimeSpan.FromSeconds(DurationSeconds.Value);
            if (StartDate is not null && EndDate is not null && EndDate > StartDate) return EndDate.Value - StartDate.Value;
            return TimeSpan.FromHours(1);
        }
    }
}

public static class TaskStatuses
{
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public static readonly string[] All = [Pending, InProgress, Completed];

    public static string ToRussian(string status) => status switch
    {
        Pending => "Ожидает выполнения",
        InProgress => "В работе",
        Completed => "Выполнено",
        _ => status
    };
}

public static class TaskVisualStates
{
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
    public const string Overdue = "Overdue";
    public const string Completed = "Completed";
}
