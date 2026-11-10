using System.Collections.ObjectModel;
using System.Globalization;
using Planner.App.Infrastructure;
using Planner.App.Services;
using Planner.Core.Models;
using Planner.Data.Configuration;
using Planner.Data.Repositories;
using Planner.Data.Services;

namespace Planner.App.ViewModels;

public sealed class MainViewModel:ObservableObject
{
    private readonly PlannerSettings _settings;
    private readonly TaskService _tasks;
    private readonly UserAdminService _admin;
    private readonly ChangeLogRepository _changes;
    private readonly RecurringTaskProcessor _recurrence;
    private readonly IDialogService _dialogs;
    private readonly INotificationService _notifications;

    // Последняя выборка из базы. Фильтр по тексту и статусу применяется к ней локально,
    // поэтому набор символов в поиске не создаёт запросов к общей сетевой базе.
    private IReadOnlyList<TaskItem> _loadedTasks=Array.Empty<TaskItem>();
    private IReadOnlyList<TaskItem> _loadedTemplates=Array.Empty<TaskItem>();

    public User LoggedUser{get;}
    public bool IsAdmin=>LoggedUser.Role==UserRoles.Admin;
    public int SnapMinutes=>_settings.SnapMinutes;
    public IReadOnlyList<int> NewTaskDurationOptions { get; } = [15,30,45,60,90,120];
    private int _selectedNewTaskDurationMinutes=60;
    public int SelectedNewTaskDurationMinutes { get=>_selectedNewTaskDurationMinutes; set=>Set(ref _selectedNewTaskDurationMinutes,value); }
    public ObservableCollection<User> AccessibleUsers{get;}=new();
    public ObservableCollection<UserTreeNodeViewModel> UserRoots{get;}=new();
    public ObservableCollection<TaskItem> RecurringTemplates{get;}=new();
    public ObservableCollection<TaskItem> DateOnlyTasks{get;}=new();
    public ObservableCollection<CalendarTaskViewModel> CalendarTasks{get;}=new();
    public ObservableCollection<DateTime> VisibleDays{get;}=new();
    public ObservableCollection<MonthDayViewModel> MonthDays{get;}=new();
    public ObservableCollection<User> AdminUsers{get;}=new();

    private User? _selectedUser;
    public User? SelectedUser{get=>_selectedUser;private set{if(Set(ref _selectedUser,value))Raise(nameof(SelectedUserCaption));}}
    public string SelectedUserCaption=>SelectedUser?.DisplayName??"—";
    private TaskItem? _selectedTask;
    public TaskItem? SelectedTask{get=>_selectedTask;set{if(Set(ref _selectedTask,value))EditTaskCommand?.Raise();}}
    private User? _selectedAdminUser; public User? SelectedAdminUser{get=>_selectedAdminUser;set{if(Set(ref _selectedAdminUser,value))EditUserCommand?.Raise();}}

    // Отдельное свойство для списка шаблонов: список не должен сбрасывать выделение задачи,
    // выбранной в календаре, при каждом обновлении данных.
    private TaskItem? _selectedTemplate;
    public TaskItem? SelectedTemplate{get=>_selectedTemplate;set{if(Set(ref _selectedTemplate,value)&&value is not null)SelectedTask=value;}}

    private string? _searchText;
    public string? SearchText{get=>_searchText;set{if(Set(ref _searchText,value))ApplyFilter();}}
    private bool _hideCompleted;
    public bool HideCompleted{get=>_hideCompleted;set{if(Set(ref _hideCompleted,value))ApplyFilter();}}

    private CalendarViewMode _viewMode=CalendarViewMode.WorkWeek;
    public CalendarViewMode ViewMode{get=>_viewMode;private set{if(Set(ref _viewMode,value)){Raise(nameof(IsMonthView));Raise(nameof(IsTimeView));Raise(nameof(ViewModeCaption));}}}
    public bool IsMonthView=>ViewMode==CalendarViewMode.Month;
    public bool IsTimeView=>!IsMonthView;
    public string ViewModeCaption=>ViewMode switch{CalendarViewMode.Day=>"День",CalendarViewMode.WorkWeek=>"Рабочая неделя",_=>"Месяц"};

    private DateTime _anchorDate=DateTime.Today;
    public DateTime AnchorDate{get=>_anchorDate;private set{if(Set(ref _anchorDate,value))Raise(nameof(MonthCaption));}}
    public string MonthCaption
    {
        get
        {
            var text=AnchorDate.ToString("MMMM yyyy",CultureInfo.GetCultureInfo("ru-RU"));
            return char.ToUpper(text[0],CultureInfo.GetCultureInfo("ru-RU"))+text[1..];
        }
    }

    private string _statusText="Готово"; public string StatusText{get=>_statusText;set=>Set(ref _statusText,value);}
    private long _lastChangeId;

    // Номер последнего запущенного обновления: ответы базы могут прийти не по порядку
    // (сигнал от другого ПК + действие пользователя), применяется только самый свежий.
    private int _refreshVersion;
    // Временные идентификаторы для задач, которые уже показаны в календаре, но ещё сохраняются.
    private long _nextTempId=-1;

    public AsyncRelayCommand RefreshCommand{get;}
    public AsyncRelayCommand NewTaskCommand{get;}
    public AsyncRelayCommand EditTaskCommand{get;}
    public AsyncRelayCommand StatisticsCommand{get;}
    public AsyncRelayCommand NewUserCommand{get;}
    public AsyncRelayCommand EditUserCommand{get;}
    public AsyncRelayCommand PreviousCommand{get;}
    public AsyncRelayCommand NextCommand{get;}
    public AsyncRelayCommand TodayCommand{get;}
    public AsyncRelayCommand SetViewCommand{get;}

    public MainViewModel(PlannerSettings settings,User logged,TaskService tasks,UserAdminService admin,ChangeLogRepository changes,RecurringTaskProcessor recurrence,IDialogService dialogs,INotificationService notifications)
    {
        _settings=settings;LoggedUser=logged;_tasks=tasks;_admin=admin;_changes=changes;_recurrence=recurrence;_dialogs=dialogs;_notifications=notifications;
        RefreshCommand=new AsyncRelayCommand(()=>RefreshAsync());
        NewTaskCommand=new AsyncRelayCommand(()=>NewTaskAsync());
        EditTaskCommand=new AsyncRelayCommand(()=>EditTaskAsync(),()=>SelectedTask is not null);
        StatisticsCommand=new AsyncRelayCommand(()=>ShowStatisticsAsync());
        NewUserCommand=new AsyncRelayCommand(()=>NewUserAsync());
        EditUserCommand=new AsyncRelayCommand(()=>EditUserAsync(),()=>SelectedAdminUser is not null);
        PreviousCommand=new AsyncRelayCommand(async()=>{AnchorDate=ViewMode==CalendarViewMode.Month?AnchorDate.AddMonths(-1):AnchorDate.AddDays(ViewMode==CalendarViewMode.Day?-1:-7);await RefreshAsync();});
        NextCommand=new AsyncRelayCommand(async()=>{AnchorDate=ViewMode==CalendarViewMode.Month?AnchorDate.AddMonths(1):AnchorDate.AddDays(ViewMode==CalendarViewMode.Day?1:7);await RefreshAsync();});
        TodayCommand=new AsyncRelayCommand(async()=>{AnchorDate=DateTime.Today;await RefreshAsync();});
        SetViewCommand=new AsyncRelayCommand(async p=>{if(Enum.TryParse<CalendarViewMode>(p?.ToString(),out var m)&&m!=ViewMode){ViewMode=m;await RefreshAsync();}});
    }

    public async Task InitializeAsync(CancellationToken ct=default)
    {
        await ReloadUsersAsync(ct);
        SelectedUser=AccessibleUsers.FirstOrDefault(x=>x.Id==LoggedUser.Id)??AccessibleUsers.FirstOrDefault();
        _lastChangeId=await _changes.GetMaxIdAsync(ct);
        await RefreshAsync(ct);
    }

    public async Task SelectUserAsync(User user,CancellationToken ct=default)
    {
        if(SelectedUser?.Id==user.Id)return;
        SelectedUser=user;
        await RefreshAsync(ct);
    }

    public void SelectTask(TaskItem task)=>SelectedTask=task;
    public bool CanManageTask(TaskItem task)=>_tasks.CanManage(LoggedUser,task);

    /// <summary>Кому можно передать задачу: любой доступный сотрудник, кроме текущего исполнителя.
    /// Себя тоже видно — руководитель может забрать задачу обратно.</summary>
    public IReadOnlyList<User> GetTransferTargets(TaskItem task)=>AccessibleUsers.Where(x=>x.Id!=task.AssignedToUserId).ToList();

    public async Task RefreshAsync(CancellationToken ct=default)
    {
        if(SelectedUser is null)return;
        StatusText="Обновление…";
        try
        {
            var version=++_refreshVersion;
            var userId=SelectedUser.Id;
            var (from,to)=GetRange();
            var (tasks,templates)=await _tasks.GetCalendarAsync(LoggedUser,userId,from,to,ct);
            // Пока шёл запрос, пользователь мог переключить неделю/сотрудника или запустить новое обновление.
            if(version!=_refreshVersion)return;
            _loadedTasks=tasks;
            _loadedTemplates=templates;
            ApplyFilter();
            StatusText=$"Обновлено: {DateTime.Now:T}";
        }
        catch(OperationCanceledException){}
        catch(Exception ex){StatusText=$"Ошибка обновления: {ex.Message}";}
    }

    private void ApplyFilter()
    {
        var tasks=Filter(_loadedTasks);
        var selectedId=SelectedTask?.Id;

        VisibleDays.Clear();foreach(var d in GetVisibleDays())VisibleDays.Add(d);
        CalendarTasks.Clear();foreach(var t in tasks.Where(x=>!x.IsAllDay))CalendarTasks.Add(new CalendarTaskViewModel(t));
        DateOnlyTasks.Clear();foreach(var t in tasks.Where(x=>x.IsAllDay))DateOnlyTasks.Add(t);
        RecurringTemplates.Clear();foreach(var t in Filter(_loadedTemplates))RecurringTemplates.Add(t);
        BuildMonth(tasks);

        // Выделение переживает обновление данных: кнопка «Открыть задачу» не гаснет после синхронизации.
        if(selectedId is long id)
            SelectedTask=tasks.FirstOrDefault(x=>x.Id==id)??RecurringTemplates.FirstOrDefault(x=>x.Id==id);
    }

    private IReadOnlyList<TaskItem> Filter(IReadOnlyList<TaskItem> source)
    {
        IEnumerable<TaskItem> query=source;
        if(HideCompleted) query=query.Where(x=>x.Status!=TaskStatuses.Completed);
        var text=SearchText?.Trim();
        if(!string.IsNullOrEmpty(text))
            query=query.Where(x=>ContainsText(x.Title,text)||ContainsText(x.Description,text)||ContainsText(x.AssigneeDisplay,text)||ContainsText(x.CreatorDisplay,text));
        return query.ToList();
    }

    private static bool ContainsText(string? value,string text)=>
        !string.IsNullOrEmpty(value)&&value.Contains(text,StringComparison.CurrentCultureIgnoreCase);

    public async Task RescheduleTaskAsync(TaskItem task,DateTime start,CancellationToken ct=default)
    {
        if(task.Id<=0)return;
        // Карточка переезжает сразу, запись в сетевую базу идёт в фоне.
        var duration=task.EffectiveDuration;
        ApplyLocally(task,t=>{t.StartDate=start;t.EndDate=start+duration;t.DurationSeconds=(long)duration.TotalSeconds;t.IsAllDay=false;});
        await PersistAsync(()=>_tasks.RescheduleAsync(LoggedUser,task.Id,start,ct),"Не удалось перенести задачу",ct);
    }

    public async Task ResizeTaskAsync(TaskItem task,TimeSpan duration,CancellationToken ct=default)
    {
        if(task.Id<=0||task.StartDate is null)return;
        ApplyLocally(task,t=>{t.EndDate=t.StartDate!.Value+duration;t.DurationSeconds=(long)duration.TotalSeconds;});
        await PersistAsync(()=>_tasks.ResizeAsync(LoggedUser,task.Id,duration,ct),"Не удалось изменить длительность",ct);
    }

    /// <summary>Создание задачи из сетки: время начала и длительность выбраны протягиванием мыши.</summary>
    public Task CreateTaskAtAsync(DateTime start,int? durationMinutes=null,CancellationToken ct=default)=>
        CreateTaskAsync(start,durationMinutes??SelectedNewTaskDurationMinutes,false,ct);

    /// <summary>Создание задачи без времени — щелчок по полосе «Без времени».</summary>
    public Task CreateDateOnlyTaskAsync(DateTime date,CancellationToken ct=default)=>
        CreateTaskAsync(date.Date,SelectedNewTaskDurationMinutes,true,ct);

    /// <summary>
    /// Оптимистичное изменение: правим локальную копию задачи и сразу перерисовываем календарь.
    /// Если сохранение не удастся, следующее обновление из базы вернёт прежнее состояние.
    /// </summary>
    private void ApplyLocally(TaskItem task,Action<TaskItem> change)
    {
        change(task);
        var loaded=_loadedTasks.FirstOrDefault(x=>x.Id==task.Id);
        if(loaded is not null&&!ReferenceEquals(loaded,task))change(loaded);
        ApplyFilter();
    }

    private async Task PersistAsync(Func<Task> save,string errorTitle,CancellationToken ct)
    {
        StatusText="Сохранение…";
        try{await save();}
        catch(Exception ex){_dialogs.Error(errorTitle,ex.Message);}
        await RefreshAsync(ct);
    }

    public async Task EditSpecificTaskAsync(TaskItem task,CancellationToken ct=default){SelectedTask=task;await EditTaskAsync(ct);}

    public async Task ChangeStatusAsync(TaskItem task,string status,CancellationToken ct=default)
    {
        if(task.Id<=0)return;
        ApplyLocally(task,t=>t.Status=status);
        await PersistAsync(()=>_tasks.ChangeStatusAsync(LoggedUser,task.Id,status,ct),"Не удалось изменить статус",ct);
    }

    public async Task DeleteTaskAsync(TaskItem task,CancellationToken ct=default)
    {
        var question=task.IsRecurringTemplate
            ?$"Удалить шаблон «{task.Title}» и все его будущие повторы?"
            :$"Удалить задачу «{task.Title}»?";
        if(!_dialogs.Confirm("Удаление задачи",question))return;
        if(task.Id<=0)return;
        SelectedTask=null;
        _loadedTasks=_loadedTasks.Where(x=>x.Id!=task.Id&&x.SourceTaskId!=task.Id).ToList();
        _loadedTemplates=_loadedTemplates.Where(x=>x.Id!=task.Id).ToList();
        ApplyFilter();
        await PersistAsync(()=>_tasks.DeleteAsync(LoggedUser,task.Id,ct),"Не удалось удалить задачу",ct);
    }

    public async Task TransferTaskAsync(TaskItem task,User target,CancellationToken ct=default)
    {
        if(task.Id<=0)return;
        try
        {
            StatusText="Сохранение…";
            await _tasks.TransferAsync(LoggedUser,task.Id,target.Id,ct);
            if(task.IsRecurringTemplate)await RunRecurrenceAsync(ct);
        }
        catch(Exception ex){_dialogs.Error("Не удалось передать задачу",ex.Message);}
        await RefreshAsync(ct);
    }

    public async Task HandleExternalSignalAsync(CancellationToken ct=default)
    {
        try
        {
            var batch=await _changes.GetBatchForUserAsync(_lastChangeId,LoggedUser.Id,ct);
            _lastChangeId=Math.Max(_lastChangeId,batch.MaxId);
            // Свои же действия обратно уведомлением не возвращаются.
            foreach(var change in batch.Entries.Where(x=>x.ActorUserId!=LoggedUser.Id&&!string.IsNullOrWhiteSpace(x.Message)).TakeLast(3))
                _notifications.Show("Сетевой планировщик",change.Message!);
            await RefreshAsync(ct);
        }
        catch(OperationCanceledException){}
        catch(Exception ex){StatusText=$"Синхронизация: {ex.Message}";}
    }

    private Task NewTaskAsync(CancellationToken ct=default)=>CreateTaskAsync(AnchorDate.Date.AddHours(9),SelectedNewTaskDurationMinutes,false,ct);

    private async Task CreateTaskAsync(DateTime start,int durationMinutes,bool withoutTime,CancellationToken ct=default)
    {
        if(SelectedUser is null)return;
        TaskDraft? draft;
        try{draft=_dialogs.ShowTaskEditor(null,null,AccessibleUsers.ToList(),SelectedUser.Id,LoggedUser,start,durationMinutes,withoutTime);}
        catch(Exception ex){_dialogs.Error("Ошибка создания задачи",ex.Message);return;}
        if(draft is null)return;

        // Задача появляется в календаре сразу после закрытия окна, ещё до записи в сетевую базу.
        var placeholder=ShowPendingTask(draft);
        StatusText="Сохранение задачи…";
        try
        {
            var id=await _tasks.CreateAsync(LoggedUser,draft,ct);
            if(placeholder is not null)placeholder.Id=id;
            if(!string.IsNullOrWhiteSpace(draft.CronSchedule))await RunRecurrenceAsync(ct);
        }
        catch(Exception ex)
        {
            if(placeholder is not null){_loadedTasks=_loadedTasks.Where(x=>!ReferenceEquals(x,placeholder)).ToList();ApplyFilter();}
            _dialogs.Error("Ошибка создания задачи",ex.Message);
        }
        await RefreshAsync(ct);
    }

    private TaskItem? ShowPendingTask(TaskDraft draft)
    {
        if(SelectedUser is null||draft.StartDate is null||!string.IsNullOrWhiteSpace(draft.CronSchedule))return null;
        var (from,to)=GetRange();
        var start=draft.StartDate.Value;
        if(start<from||start>=to)return null;
        var visibleForSelected=draft.AssignedToUserId==SelectedUser.Id||(SelectedUser.Id==LoggedUser.Id);
        if(!visibleForSelected)return null;

        var item=new TaskItem
        {
            Id=_nextTempId--,Title=draft.Title,Description=draft.Description,CreatedByUserId=LoggedUser.Id,AssignedToUserId=draft.AssignedToUserId,
            StartDate=start,DurationSeconds=draft.IsAllDay?0:draft.DurationSeconds,EndDate=draft.IsAllDay?start.Date:start.AddSeconds(draft.DurationSeconds),
            IsAllDay=draft.IsAllDay,Status=draft.Status,CreatedAt=DateTime.Now,UpdatedAt=DateTime.Now,
            CreatedByDisplayName=LoggedUser.DisplayName,
            AssignedToDisplayName=AccessibleUsers.FirstOrDefault(x=>x.Id==draft.AssignedToUserId)?.DisplayName??string.Empty
        };
        _loadedTasks=_loadedTasks.Append(item).OrderBy(x=>x.StartDate).ThenByDescending(x=>x.IsAllDay).ToList();
        ApplyFilter();
        return item;
    }

    /// <summary>Генерация повторов перебирает все шаблоны на год вперёд — только в фоне и только когда нужно.</summary>
    private Task RunRecurrenceAsync(CancellationToken ct)=>Task.Run(()=>_recurrence.ProcessAsync(ct),ct);

    private async Task EditTaskAsync(CancellationToken ct=default)
    {
        if(SelectedTask is null){_dialogs.Error("Сетевой планировщик","Выберите задачу в календаре или шаблон повторения.");return;}
        var task=SelectedTask;
        try
        {
            if(task.Id<=0)return;
            var rem=await _tasks.GetReminderOffsetAsync(task.Id,ct);
            var draft=_dialogs.ShowTaskEditor(task,rem,AccessibleUsers.ToList(),task.AssignedToUserId,LoggedUser);
            if(draft is null)return;
            StatusText="Сохранение…";
            var recurringInvolved=task.IsRecurringTemplate||!string.IsNullOrWhiteSpace(draft.CronSchedule);
            await _tasks.UpdateAsync(LoggedUser,draft,ct);
            if(recurringInvolved&&task.SourceTaskId is null)await RunRecurrenceAsync(ct);
        }
        catch(Exception ex){_dialogs.Error("Ошибка сохранения задачи",ex.Message);}
        await RefreshAsync(ct);
    }

    private async Task ShowStatisticsAsync(CancellationToken ct=default)
    {
        var statistics=new StatisticsViewModel(LoggedUser,(from,to,token)=>_tasks.GetStatisticsAsync(LoggedUser,from,to,token));
        await statistics.ReloadAsync(ct);
        _dialogs.ShowStatistics(statistics);
    }

    private async Task NewUserAsync(CancellationToken ct=default)
    {
        var user=_dialogs.ShowUserEditor(null,AdminUsers.ToList());if(user is null)return;
        try{await _admin.SaveAsync(LoggedUser,user,ct);await ReloadUsersAsync(ct);await RefreshAsync(ct);}catch(Exception ex){_dialogs.Error("Ошибка пользователя",ex.Message);}
    }

    private async Task EditUserAsync(CancellationToken ct=default)
    {
        if(SelectedAdminUser is null)return;var user=_dialogs.ShowUserEditor(SelectedAdminUser,AdminUsers.ToList());if(user is null)return;
        try{await _admin.SaveAsync(LoggedUser,user,ct);await ReloadUsersAsync(ct);await RefreshAsync(ct);}catch(Exception ex){_dialogs.Error("Ошибка пользователя",ex.Message);}
    }

    private async Task ReloadUsersAsync(CancellationToken ct)
    {
        var accessible=await _tasks.GetAccessibleUsersAsync(LoggedUser,ct);AccessibleUsers.Clear();foreach(var u in accessible)AccessibleUsers.Add(u);
        if(IsAdmin){var all=await _admin.GetAllAsync(ct);AdminUsers.Clear();foreach(var u in all)AdminUsers.Add(u);}BuildTree();
    }

    private void BuildTree()
    {
        UserRoots.Clear();var nodes=AccessibleUsers.ToDictionary(x=>x.Id,x=>new UserTreeNodeViewModel(x));
        foreach(var node in nodes.Values){if(node.User.ParentId is long p&&nodes.TryGetValue(p,out var parent))parent.Children.Add(node);else UserRoots.Add(node);}
    }

    private IEnumerable<DateTime> GetVisibleDays()
    {
        if(ViewMode==CalendarViewMode.Day){yield return AnchorDate.Date;yield break;}
        var start=StartOfWeek(AnchorDate,DayOfWeek.Monday);for(var i=0;i<5;i++)yield return start.AddDays(i);
    }

    private (DateTime from,DateTime to) GetRange()
    {
        if(ViewMode==CalendarViewMode.Day)return(AnchorDate.Date,AnchorDate.Date.AddDays(1));
        if(ViewMode==CalendarViewMode.WorkWeek){var m=StartOfWeek(AnchorDate,DayOfWeek.Monday);return(m,m.AddDays(5));}
        var first=new DateTime(AnchorDate.Year,AnchorDate.Month,1);var grid=StartOfWeek(first,DayOfWeek.Monday);return(grid,grid.AddDays(42));
    }

    private void BuildMonth(IReadOnlyList<TaskItem> tasks)
    {
        MonthDays.Clear();if(ViewMode!=CalendarViewMode.Month)return;
        var first=new DateTime(AnchorDate.Year,AnchorDate.Month,1);var grid=StartOfWeek(first,DayOfWeek.Monday);
        for(var i=0;i<42;i++){var d=grid.AddDays(i);var vm=new MonthDayViewModel{Date=d,IsCurrentMonth=d.Month==AnchorDate.Month};foreach(var t in tasks.Where(x=>x.StartDate?.Date==d.Date))vm.Tasks.Add(t);MonthDays.Add(vm);}
    }

    private static DateTime StartOfWeek(DateTime date,DayOfWeek start){var diff=(7+(date.DayOfWeek-start))%7;return date.Date.AddDays(-diff);}
}
