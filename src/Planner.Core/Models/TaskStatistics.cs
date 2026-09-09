namespace Planner.Core.Models;

/// <summary>
/// Строка статистики: либо конкретный сотрудник, либо итоговая строка «Итого».
/// Просрочка считается тем же правилом, что и в календаре (<see cref="TaskItem.IsOverdue"/>),
/// поэтому цифры отчёта всегда совпадают с цветами карточек.
/// </summary>
public sealed class TaskStatisticsRow
{
    public long UserId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public bool IsTotal { get; init; }

    /// <summary>Всего задач, назначенных сотруднику за период.</summary>
    public int Total { get; private set; }
    public int Pending { get; private set; }
    public int InProgress { get; private set; }
    public int Completed { get; private set; }
    public int Overdue { get; private set; }
    /// <summary>Сотрудник поставил задачу себе сам.</summary>
    public int Own { get; private set; }
    /// <summary>Задачу поручил кто-то другой (руководитель или администратор).</summary>
    public int Delegated { get; private set; }

    public double CompletionRate => Total == 0 ? 0 : Math.Round(Completed * 100.0 / Total, 1);
    public double OverdueRate => Total == 0 ? 0 : Math.Round(Overdue * 100.0 / Total, 1);
    public bool HasOverdue => Overdue > 0;
    public string CompletionRateDisplay => Total == 0 ? "—" : $"{CompletionRate:0.#} %";
    public string OverdueRateDisplay => Total == 0 ? "—" : $"{OverdueRate:0.#} %";

    /// <summary>Учитывает задачу в строке. <paramref name="ownerUserId"/> — сотрудник, которому она назначена.</summary>
    public void Add(TaskItem task, long ownerUserId)
    {
        Total++;
        if (task.Status == TaskStatuses.Completed) Completed++;
        else if (task.Status == TaskStatuses.InProgress) InProgress++;
        else Pending++;

        if (task.IsOverdue) Overdue++;
        if (task.CreatedByUserId == ownerUserId) Own++; else Delegated++;
    }
}

/// <summary>Готовый отчёт: строки по сотрудникам плюс общая строка.</summary>
public sealed class TaskStatisticsReport
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public IReadOnlyList<TaskStatisticsRow> Rows { get; init; } = Array.Empty<TaskStatisticsRow>();
    public TaskStatisticsRow Total { get; init; } = new() { DisplayName = "Итого", IsTotal = true };
}
