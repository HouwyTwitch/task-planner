using Cronos;
using Planner.Core.Models;
using Planner.Data.Repositories;

namespace Planner.Data.Services;

public sealed class TaskService
{
    private readonly UserRepository _users;
    private readonly TaskRepository _tasks;
    private readonly SignalService _signals;

    public TaskService(UserRepository users, TaskRepository tasks, SignalService signals)
    {
        _users=users; _tasks=tasks; _signals=signals;
    }

    public async Task<IReadOnlyList<User>> GetAccessibleUsersAsync(User loggedUser, CancellationToken ct=default) =>
        loggedUser.Role == UserRoles.Admin ? await _users.GetAllAsync(ct) : await _users.GetSelfAndSubordinatesAsync(loggedUser.Id, ct);

    public async Task<IReadOnlyList<TaskItem>> GetTasksAsync(User loggedUser,long targetUserId,DateTime from,DateTime to,CancellationToken ct=default)
    {
        await EnsureCanViewAsync(loggedUser,targetUserId,ct);
        return await _tasks.GetForUserAsync(targetUserId,from,to,targetUserId==loggedUser.Id,ct);
    }

    public async Task<IReadOnlyList<TaskItem>> GetRecurringTemplatesAsync(User loggedUser,long targetUserId,CancellationToken ct=default)
    {
        await EnsureCanViewAsync(loggedUser,targetUserId,ct);
        return await _tasks.GetRecurringTemplatesAsync(targetUserId,targetUserId==loggedUser.Id,ct);
    }

    /// <summary>
    /// Отчёт по задачам за период: строка на каждого доступного сотрудника плюс общая строка.
    /// Руководитель видит себя и всех подчинённых любого уровня, администратор — всех.
    /// </summary>
    public async Task<TaskStatisticsReport> GetStatisticsAsync(User loggedUser,DateTime from,DateTime to,CancellationToken ct=default)
    {
        if(to<=from) throw new ArgumentException("Конец периода должен быть позже начала.");
        var users=await GetAccessibleUsersAsync(loggedUser,ct);
        var rows=new Dictionary<long,TaskStatisticsRow>();
        foreach(var user in users) rows[user.Id]=new TaskStatisticsRow{UserId=user.Id,DisplayName=user.DisplayName};
        var total=new TaskStatisticsRow{DisplayName="Итого",IsTotal=true};

        if(rows.Count>0)
        {
            var tasks=await _tasks.GetForUsersAsync(rows.Keys.ToList(),from,to,ct);
            foreach(var task in tasks)
            {
                if(rows.TryGetValue(task.AssignedToUserId,out var row)) row.Add(task,task.AssignedToUserId);
                total.Add(task,task.AssignedToUserId);
            }
        }

        return new TaskStatisticsReport
        {
            From=from,To=to,Total=total,
            Rows=rows.Values.OrderByDescending(x=>x.UserId==loggedUser.Id).ThenBy(x=>x.DisplayName,StringComparer.CurrentCulture).ToList()
        };
    }

    public async Task<long> CreateAsync(User loggedUser, TaskDraft draft, CancellationToken ct=default)
    {
        ValidateDraft(draft); ValidateCron(draft.CronSchedule);
        await EnsureCanViewAsync(loggedUser,draft.AssignedToUserId,ct);
        var start=draft.StartDate!.Value;
        var item=new TaskItem
        {
            Title=draft.Title,Description=draft.Description,CreatedByUserId=loggedUser.Id,AssignedToUserId=draft.AssignedToUserId,StartDate=start,
            DurationSeconds=draft.IsAllDay?0:draft.DurationSeconds,EndDate=draft.IsAllDay?start.Date:start.AddSeconds(draft.DurationSeconds),IsAllDay=draft.IsAllDay,
            CronSchedule=string.IsNullOrWhiteSpace(draft.CronSchedule)?null:draft.CronSchedule.Trim(),Status=draft.Status
        };
        var message=loggedUser.Id==draft.AssignedToUserId ? $"Создана задача «{draft.Title}»" : $"{loggedUser.DisplayName} назначил(а) вам задачу «{draft.Title}»";
        var id=await _tasks.CreateAsync(item,draft.ReminderOffsetSeconds,loggedUser.Id,loggedUser.Role==UserRoles.Admin,message,ct);
        if(draft.AssignedToUserId!=loggedUser.Id) await SafeSignalAsync(draft.AssignedToUserId,ct);
        return id;
    }

    public async Task UpdateAsync(User loggedUser, TaskDraft draft, CancellationToken ct=default)
    {
        if(draft.Id is null) throw new ArgumentException("Идентификатор задачи отсутствует.");
        var existing=await _tasks.GetByIdAsync(draft.Id.Value,ct) ?? throw new KeyNotFoundException("Задача не найдена.");
        var canManage=loggedUser.Role==UserRoles.Admin || existing.CreatedByUserId==loggedUser.Id;

        if(!canManage)
        {
            if(existing.AssignedToUserId!=loggedUser.Id) throw new UnauthorizedAccessException("Нет права изменять эту задачу.");
            await _tasks.UpdateProgressAsync(existing.Id,draft.Description,draft.Status,loggedUser.Id,false,existing.CreatedByUserId,ct);
            if(existing.CreatedByUserId!=loggedUser.Id) await SafeSignalAsync(existing.CreatedByUserId,ct);
            return;
        }

        ValidateDraft(draft); ValidateCron(draft.CronSchedule);
        var oldAssignee=existing.AssignedToUserId;
        var wasRecurring=existing.SourceTaskId is null && !string.IsNullOrWhiteSpace(existing.CronSchedule);
        existing.Title=draft.Title; existing.Description=draft.Description; existing.AssignedToUserId=draft.AssignedToUserId; existing.StartDate=draft.StartDate;
        existing.DurationSeconds=draft.IsAllDay?0:draft.DurationSeconds; existing.EndDate=draft.IsAllDay?draft.StartDate!.Value.Date:draft.StartDate!.Value.AddSeconds(draft.DurationSeconds);
        existing.IsAllDay=draft.IsAllDay; existing.CronSchedule=string.IsNullOrWhiteSpace(draft.CronSchedule)?null:draft.CronSchedule.Trim(); existing.Status=draft.Status;
        await _tasks.UpdateManagedAsync(existing,draft.ReminderOffsetSeconds,loggedUser.Id,loggedUser.Role==UserRoles.Admin,$"Задача «{draft.Title}» изменена",ct);

        if(existing.SourceTaskId is null && (wasRecurring || !string.IsNullOrWhiteSpace(existing.CronSchedule)))
            await _tasks.DeleteFutureGeneratedAsync(existing.Id,DateTime.Today,ct);

        if(oldAssignee!=existing.AssignedToUserId) await SafeSignalAsync(oldAssignee,ct);
        if(existing.AssignedToUserId!=loggedUser.Id) await SafeSignalAsync(existing.AssignedToUserId,ct);
    }

    public async Task RescheduleAsync(User loggedUser, long taskId, DateTime start, CancellationToken ct=default)
    {
        var task=await _tasks.GetByIdAsync(taskId,ct) ?? throw new KeyNotFoundException("Задача не найдена.");
        if(loggedUser.Role!=UserRoles.Admin && task.CreatedByUserId!=loggedUser.Id)
            throw new UnauthorizedAccessException("Переносить задачу по календарю может только её создатель или администратор.");
        var end=start+task.EffectiveDuration;
        await _tasks.UpdateScheduleAsync(taskId,start,end,loggedUser.Id,loggedUser.Role==UserRoles.Admin,task.AssignedToUserId,ct);
        if(task.AssignedToUserId!=loggedUser.Id) await SafeSignalAsync(task.AssignedToUserId,ct);
    }


    public async Task ResizeAsync(User loggedUser, long taskId, TimeSpan duration, CancellationToken ct=default)
    {
        if(duration <= TimeSpan.Zero) throw new ArgumentException("Длительность задачи должна быть больше нуля.");
        var task=await _tasks.GetByIdAsync(taskId,ct) ?? throw new KeyNotFoundException("Задача не найдена.");
        if(task.StartDate is null || task.IsAllDay) throw new InvalidOperationException("Растягивать можно только задачу со временем.");
        if(loggedUser.Role!=UserRoles.Admin && task.CreatedByUserId!=loggedUser.Id)
            throw new UnauthorizedAccessException("Изменять длительность задачи может только её создатель или администратор.");
        var end=task.StartDate.Value+duration;
        await _tasks.UpdateScheduleAsync(taskId,task.StartDate.Value,end,loggedUser.Id,loggedUser.Role==UserRoles.Admin,task.AssignedToUserId,ct);
        if(task.AssignedToUserId!=loggedUser.Id) await SafeSignalAsync(task.AssignedToUserId,ct);
    }

    public async Task ChangeStatusAsync(User loggedUser,long taskId,string status,CancellationToken ct=default)
    {
        if(!TaskStatuses.All.Contains(status)) throw new ArgumentException("Некорректный статус.");
        var task=await _tasks.GetByIdAsync(taskId,ct) ?? throw new KeyNotFoundException("Задача не найдена.");
        var other=loggedUser.Id==task.AssignedToUserId?task.CreatedByUserId:task.AssignedToUserId;
        await _tasks.UpdateStatusAsync(taskId,status,loggedUser.Id,loggedUser.Role==UserRoles.Admin,other,ct);
        if(other!=loggedUser.Id) await SafeSignalAsync(other,ct);
    }

    public async Task TransferAsync(User loggedUser,long taskId,long newAssigneeId,CancellationToken ct=default)
    {
        var task=await _tasks.GetByIdAsync(taskId,ct) ?? throw new KeyNotFoundException("Задача не найдена.");
        if(loggedUser.Role!=UserRoles.Admin && task.CreatedByUserId!=loggedUser.Id)
            throw new UnauthorizedAccessException("Передавать задачу может только её создатель или администратор.");
        var old=task.AssignedToUserId;
        await _tasks.TransferAsync(taskId,newAssigneeId,loggedUser.Id,loggedUser.Role==UserRoles.Admin,ct);
        if(task.SourceTaskId is null && !string.IsNullOrWhiteSpace(task.CronSchedule)) await _tasks.DeleteFutureGeneratedAsync(task.Id,DateTime.Today,ct);
        if(old!=loggedUser.Id) await SafeSignalAsync(old,ct);
        if(newAssigneeId!=loggedUser.Id) await SafeSignalAsync(newAssigneeId,ct);
    }

    public async Task DeleteAsync(User loggedUser,long taskId,CancellationToken ct=default)
    {
        var task=await _tasks.GetByIdAsync(taskId,ct) ?? throw new KeyNotFoundException("Задача не найдена.");
        if(loggedUser.Role!=UserRoles.Admin && task.CreatedByUserId!=loggedUser.Id)
            throw new UnauthorizedAccessException("Удалять задачу может только её создатель или администратор.");
        await _tasks.DeleteAsync(taskId,loggedUser.Id,loggedUser.Role==UserRoles.Admin,task.AssignedToUserId,ct);
        if(task.AssignedToUserId!=loggedUser.Id) await SafeSignalAsync(task.AssignedToUserId,ct);
    }

    public bool CanManage(User loggedUser,TaskItem task) => loggedUser.Role==UserRoles.Admin || task.CreatedByUserId==loggedUser.Id;

    public Task<TaskItem?> GetByIdAsync(long id,CancellationToken ct=default)=>_tasks.GetByIdAsync(id,ct);
    public Task<long?> GetReminderOffsetAsync(long id,CancellationToken ct=default)=>_tasks.GetReminderOffsetAsync(id,ct);

    private static void ValidateDraft(TaskDraft draft)
    {
        if(string.IsNullOrWhiteSpace(draft.Title)) throw new ArgumentException("Название задачи обязательно.");
        if(draft.StartDate is null) throw new ArgumentException("Дата задачи обязательна.");
        if(!draft.IsAllDay && draft.DurationSeconds<=0) throw new ArgumentException("Для задачи со временем длительность должна быть больше нуля.");
    }

    private static void ValidateCron(string? cron)
    {
        if(string.IsNullOrWhiteSpace(cron)) return;
        try { _=CronExpression.Parse(cron); }
        catch(CronFormatException ex) { throw new ArgumentException($"Некорректное Cron-выражение: {ex.Message}", nameof(cron), ex); }
    }

    private async Task EnsureCanViewAsync(User logged,long target,CancellationToken ct)
    {
        if(logged.Role==UserRoles.Admin || logged.Id==target) return;
        if(!await _users.IsSubordinateAsync(logged.Id,target,ct)) throw new UnauthorizedAccessException("Нет доступа к выбранному сотруднику.");
    }

    private async Task SafeSignalAsync(long userId,CancellationToken ct)
    {
        try{await _signals.SignalUserAsync(userId,ct);}catch{}
    }
}
