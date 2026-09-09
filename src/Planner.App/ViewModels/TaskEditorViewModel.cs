using System.Collections.ObjectModel;
using Planner.App.Infrastructure;
using Planner.Core.Models;
using Planner.Core.Services;

namespace Planner.App.ViewModels;

public sealed record StatusOption(string Code,string Name);
public sealed record DayOption(DayOfWeek Value,string Name);

public sealed class TaskEditorViewModel : ObservableObject
{
    private readonly TaskItem? _existing;
    private readonly bool _canEditCore;

    public ObservableCollection<User> Users { get; }
    public ObservableCollection<ReminderUnit> ReminderUnits { get; }=new(ReminderTimeConverter.Units);
    public StatusOption[] Statuses { get; }=
    [
        new(TaskStatuses.Pending,"Ожидает"),
        new(TaskStatuses.InProgress,"В работе"),
        new(TaskStatuses.Completed,"Выполнена")
    ];
    public string[] RecurrenceTypes { get; }=["Нет","Ежедневно","По будням","Еженедельно","Ежемесячно","Ежеквартально","Раз в полугодие","Ежегодно","Cron"];
    public DayOption[] Days { get; }=
    [
        new(DayOfWeek.Monday,"Понедельник"),new(DayOfWeek.Tuesday,"Вторник"),new(DayOfWeek.Wednesday,"Среда"),
        new(DayOfWeek.Thursday,"Четверг"),new(DayOfWeek.Friday,"Пятница"),new(DayOfWeek.Saturday,"Суббота"),new(DayOfWeek.Sunday,"Воскресенье")
    ];

    public bool CanEditCore => _canEditCore;
    public bool IsLimitedEdit => _existing is not null && !_canEditCore;

    /// <summary>Расписание задаётся только в шаблоне: у сгенерированного экземпляра его менять нельзя.</summary>
    public bool CanEditRecurrence => _canEditCore && _existing?.SourceTaskId is null;
    public bool IsWeeklyRecurrence => CanEditRecurrence && RecurrenceType == "Еженедельно";
    public bool IsCronRecurrence => CanEditRecurrence && RecurrenceType == "Cron";

    public string EditModeHint
    {
        get
        {
            if (IsLimitedEdit) return "Задача назначена руководителем. Вы можете изменять описание хода выполнения и статус.";
            if (_existing?.SourceTaskId is not null) return "Это повтор задачи. Расписание повторения меняется в шаблоне на левой панели.";
            return string.Empty;
        }
    }

    public bool HasEditModeHint => !string.IsNullOrEmpty(EditModeHint);

    /// <summary>Пояснение к Cron-выражению обычными словами.</summary>
    public string RecurrenceHint =>
        IsCronRecurrence && !string.IsNullOrWhiteSpace(Cron) ? $"Будет повторяться {CronDescriber.Describe(Cron)}" : string.Empty;

    private string _title=""; public string Title{get=>_title;set=>Set(ref _title,value);}
    private string? _description; public string? Description{get=>_description;set=>Set(ref _description,value);}
    private User? _assignee; public User? Assignee{get=>_assignee;set=>Set(ref _assignee,value);}
    private DateTime? _date=DateTime.Today; public DateTime? Date{get=>_date;set=>Set(ref _date,value);}
    private string _time="09:00"; public string Time{get=>_time;set=>Set(ref _time,value);}
    private int _durationMinutes=60; public int DurationMinutes{get=>_durationMinutes;set=>Set(ref _durationMinutes,value);}
    private bool _withoutTime; public bool WithoutTime{get=>_withoutTime;set{if(Set(ref _withoutTime,value))Raise(nameof(HasTime));}}
    public bool HasTime=>!WithoutTime;
    private StatusOption _selectedStatus; public StatusOption SelectedStatus{get=>_selectedStatus;set=>Set(ref _selectedStatus,value);}
    private long _reminderValue=15; public long ReminderValue{get=>_reminderValue;set=>Set(ref _reminderValue,value);}
    private ReminderUnit _reminderUnit=ReminderTimeConverter.Units[1]; public ReminderUnit ReminderUnit{get=>_reminderUnit;set=>Set(ref _reminderUnit,value);}
    private bool _hasReminder; public bool HasReminder{get=>_hasReminder;set=>Set(ref _hasReminder,value);}
    private string _recurrenceType="Нет";
    public string RecurrenceType
    {
        get=>_recurrenceType;
        set{if(Set(ref _recurrenceType,value)){Raise(nameof(IsWeeklyRecurrence));Raise(nameof(IsCronRecurrence));Raise(nameof(RecurrenceHint));}}
    }
    private DayOption _weeklyDay; public DayOption WeeklyDay{get=>_weeklyDay;set=>Set(ref _weeklyDay,value);}
    private string? _cron;
    public string? Cron{get=>_cron;set{if(Set(ref _cron,value))Raise(nameof(RecurrenceHint));}}

    public TaskEditorViewModel(TaskItem? existing,long? reminderOffset,IReadOnlyList<User> users,long defaultAssignee,User loggedUser,DateTime? defaultStart=null,int defaultDurationMinutes=60)
    {
        _existing=existing;
        _canEditCore=existing is null || loggedUser.Role==UserRoles.Admin || existing.CreatedByUserId==loggedUser.Id;
        Users=new(users);
        _selectedStatus=Statuses[0];
        _weeklyDay=Days[0];
        Assignee=Users.FirstOrDefault(x=>x.Id==(existing?.AssignedToUserId??defaultAssignee))??Users.FirstOrDefault();

        if(existing is null)
        {
            var seed=defaultStart ?? DateTime.Today.AddHours(9);
            Date=seed.Date;
            Time=seed.ToString("HH:mm");
            DurationMinutes=Math.Max(1,defaultDurationMinutes);
        }

        if(existing is not null)
        {
            Title=existing.Title; Description=existing.Description; Date=existing.StartDate?.Date ?? DateTime.Today;
            Time=existing.StartDate?.ToString("HH:mm")??"09:00"; DurationMinutes=(int)Math.Max(1,existing.EffectiveDuration.TotalMinutes);
            WithoutTime=existing.IsAllDay; SelectedStatus=Statuses.FirstOrDefault(x=>x.Code==existing.Status)??Statuses[0]; Cron=existing.CronSchedule;
            RecurrenceType=string.IsNullOrWhiteSpace(Cron)?"Нет":"Cron";
        }
        if(reminderOffset is not null){HasReminder=true;var d=ReminderTimeConverter.FromSeconds(reminderOffset.Value);ReminderValue=d.Value;ReminderUnit=d.Unit;}
    }

    public TaskDraft BuildDraft()
    {
        if(IsLimitedEdit && _existing is not null)
        {
            return new TaskDraft
            {
                Id=_existing.Id,Title=_existing.Title,Description=Description,AssignedToUserId=_existing.AssignedToUserId,
                StartDate=_existing.StartDate,DurationSeconds=_existing.DurationSeconds??3600,IsAllDay=_existing.IsAllDay,
                CronSchedule=_existing.CronSchedule,Status=SelectedStatus.Code,ReminderOffsetSeconds=HasReminder?ReminderTimeConverter.ToSeconds(ReminderValue,ReminderUnit):null
            };
        }

        if(string.IsNullOrWhiteSpace(Title)) throw new InvalidOperationException("Введите название задачи.");
        if(Assignee is null) throw new InvalidOperationException("Выберите исполнителя.");
        if(Date is null) throw new InvalidOperationException("Дата задачи обязательна.");

        var time = WithoutTime ? new TimeOnly(0,0) : (TimeOnly.TryParse(Time,out var parsedTime) ? parsedTime : throw new InvalidOperationException("Введите время в формате ЧЧ:ММ или включите «Без времени»."));
        var start = WithoutTime ? Date.Value.Date : Date.Value.Date + time.ToTimeSpan();
        var cronTime = WithoutTime ? new TimeOnly(0,0) : time;
        var recurrenceDate=Date.Value.Date;

        var cron=!CanEditRecurrence ? _existing?.CronSchedule : RecurrenceType switch
        {
            "Нет"=>null,
            "Ежедневно"=>SimpleCronBuilder.Daily(cronTime),
            "По будням"=>SimpleCronBuilder.Weekdays(cronTime),
            "Еженедельно"=>SimpleCronBuilder.Weekly(WeeklyDay.Value,cronTime),
            "Ежемесячно"=>SimpleCronBuilder.Monthly(recurrenceDate.Day,cronTime),
            "Ежеквартально"=>SimpleCronBuilder.Quarterly(recurrenceDate,cronTime),
            "Раз в полугодие"=>SimpleCronBuilder.SemiAnnual(recurrenceDate,cronTime),
            "Ежегодно"=>SimpleCronBuilder.Annual(recurrenceDate,cronTime),
            _=>string.IsNullOrWhiteSpace(Cron)?throw new InvalidOperationException("Введите Cron-выражение."):Cron.Trim()
        };

        return new TaskDraft
        {
            Id=_existing?.Id,Title=Title.Trim(),Description=Description,AssignedToUserId=Assignee.Id,StartDate=start,
            DurationSeconds=WithoutTime?0:Math.Max(1,DurationMinutes)*60L,IsAllDay=WithoutTime,CronSchedule=cron,
            Status=SelectedStatus.Code,ReminderOffsetSeconds=HasReminder?ReminderTimeConverter.ToSeconds(ReminderValue,ReminderUnit):null
        };
    }

}
