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
    private TaskItem? _selectedTask; public TaskItem? SelectedTask{get=>_selectedTask;set=>Set(ref _selectedTask,value);}
    private User? _selectedAdminUser; public User? SelectedAdminUser{get=>_selectedAdminUser;set=>Set(ref _selectedAdminUser,value);}

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

    public AsyncRelayCommand RefreshCommand{get;}
    public AsyncRelayCommand NewTaskCommand{get;}
    public AsyncRelayCommand EditTaskCommand{get;}
    public AsyncRelayCommand NewUserCommand{get;}
    public AsyncRelayCommand EditUserCommand{get;}
    public AsyncRelayCommand PreviousCommand{get;}
    public AsyncRelayCommand NextCommand{get;}
    public AsyncRelayCommand TodayCommand{get;}
    public AsyncRelayCommand SetViewCommand{get;}

    public MainViewModel(PlannerSettings settings,User logged,TaskService tasks,UserAdminService admin,ChangeLogRepository changes,RecurringTaskProcessor recurrence,IDialogService dialogs,INotificationService notifications)
    {
        _settings=settings;LoggedUser=logged;_tasks=tasks;_admin=admin;_changes=changes;_recurrence=recurrence;_dialogs=dialogs;_notifications=notifications;
        RefreshCommand=new AsyncRelayCommand(()=>RefreshAsync());NewTaskCommand=new AsyncRelayCommand(()=>NewTaskAsync());EditTaskCommand=new AsyncRelayCommand(()=>EditTaskAsync());NewUserCommand=new AsyncRelayCommand(()=>NewUserAsync());EditUserCommand=new AsyncRelayCommand(()=>EditUserAsync());
        PreviousCommand=new AsyncRelayCommand(async()=>{AnchorDate=ViewMode==CalendarViewMode.Month?AnchorDate.AddMonths(-1):AnchorDate.AddDays(ViewMode==CalendarViewMode.Day?-1:-7);await RefreshAsync();});
        NextCommand=new AsyncRelayCommand(async()=>{AnchorDate=ViewMode==CalendarViewMode.Month?AnchorDate.AddMonths(1):AnchorDate.AddDays(ViewMode==CalendarViewMode.Day?1:7);await RefreshAsync();});
        TodayCommand=new AsyncRelayCommand(async()=>{AnchorDate=DateTime.Today;await RefreshAsync();});
        SetViewCommand=new AsyncRelayCommand(async p=>{if(Enum.TryParse<CalendarViewMode>(p?.ToString(),out var m)){ViewMode=m;await RefreshAsync();}});
    }

    public async Task InitializeAsync(CancellationToken ct=default)
    {
        await ReloadUsersAsync(ct);
        SelectedUser=AccessibleUsers.FirstOrDefault(x=>x.Id==LoggedUser.Id)??AccessibleUsers.FirstOrDefault();
        _lastChangeId=await _changes.GetMaxIdAsync(ct);
        await RefreshAsync(ct);
    }

    public async Task SelectUserAsync(User user,CancellationToken ct=default){SelectedUser=user;await RefreshAsync(ct);}
    public void SelectTask(TaskItem task)=>SelectedTask=task;
    public bool CanManageTask(TaskItem task)=>_tasks.CanManage(LoggedUser,task);
    public IReadOnlyList<User> GetTransferTargets(TaskItem task)=>AccessibleUsers.Where(x=>x.Id!=LoggedUser.Id&&x.Id!=task.AssignedToUserId).ToList();

    public async Task RefreshAsync(CancellationToken ct=default)
    {
        if(SelectedUser is null)return;
        StatusText="Обновление…";
        try
        {
            var (from,to)=GetRange();
            var tasks=await _tasks.GetTasksAsync(LoggedUser,SelectedUser.Id,from,to,ct);
            var templates=await _tasks.GetRecurringTemplatesAsync(LoggedUser,SelectedUser.Id,ct);
            VisibleDays.Clear();foreach(var d in GetVisibleDays())VisibleDays.Add(d);
            CalendarTasks.Clear();foreach(var t in tasks.Where(x=>!x.IsAllDay))CalendarTasks.Add(new CalendarTaskViewModel(t));
            DateOnlyTasks.Clear();foreach(var t in tasks.Where(x=>x.IsAllDay))DateOnlyTasks.Add(t);
            RecurringTemplates.Clear();foreach(var t in templates)RecurringTemplates.Add(t);
            BuildMonth(tasks);
            StatusText=$"Обновлено: {DateTime.Now:T}";
        }
        catch(Exception ex){StatusText=$"Ошибка обновления: {ex.Message}";}
    }

    public async Task RescheduleTaskAsync(TaskItem task,DateTime start,CancellationToken ct=default)
    {
        try{await _tasks.RescheduleAsync(LoggedUser,task.Id,start,ct);await RefreshAsync(ct);}catch(Exception ex){_notifications.Show("Не удалось перенести задачу",ex.Message);}
    }

    public async Task ResizeTaskAsync(TaskItem task,TimeSpan duration,CancellationToken ct=default)
    {
        try{await _tasks.ResizeAsync(LoggedUser,task.Id,duration,ct);await RefreshAsync(ct);}catch(Exception ex){_notifications.Show("Не удалось изменить длительность",ex.Message);}
    }

    public async Task CreateTaskAtAsync(DateTime start,CancellationToken ct=default)
    {
        if(SelectedUser is null)return;
        var draft=_dialogs.ShowTaskEditor(null,null,AccessibleUsers.ToList(),SelectedUser.Id,LoggedUser,start,SelectedNewTaskDurationMinutes);
        if(draft is null)return;
        try
        {
            await _tasks.CreateAsync(LoggedUser,draft,ct);
            if(!string.IsNullOrWhiteSpace(draft.CronSchedule))await _recurrence.ProcessAsync(ct);
            await RefreshAsync(ct);
        }
        catch(Exception ex){_notifications.Show("Ошибка создания",ex.Message);}
    }

    public async Task EditSpecificTaskAsync(TaskItem task,CancellationToken ct=default){SelectedTask=task;await EditTaskAsync(ct);}

    public async Task ChangeStatusAsync(TaskItem task,string status,CancellationToken ct=default)
    {
        try{await _tasks.ChangeStatusAsync(LoggedUser,task.Id,status,ct);await RefreshAsync(ct);}catch(Exception ex){_notifications.Show("Не удалось изменить статус",ex.Message);}
    }

    public async Task DeleteTaskAsync(TaskItem task,CancellationToken ct=default)
    {
        if(!_dialogs.Confirm("Удаление задачи",$"Удалить задачу «{task.Title}»?"))return;
        try{await _tasks.DeleteAsync(LoggedUser,task.Id,ct);SelectedTask=null;await RefreshAsync(ct);}catch(Exception ex){_notifications.Show("Не удалось удалить задачу",ex.Message);}
    }

    public async Task TransferTaskAsync(TaskItem task,User target,CancellationToken ct=default)
    {
        try{await _tasks.TransferAsync(LoggedUser,task.Id,target.Id,ct);await _recurrence.ProcessAsync(ct);await RefreshAsync(ct);}catch(Exception ex){_notifications.Show("Не удалось передать задачу",ex.Message);}
    }

    public async Task HandleExternalSignalAsync(CancellationToken ct=default)
    {
        try
        {
            var batch=await _changes.GetBatchForUserAsync(_lastChangeId,LoggedUser.Id,ct);
            _lastChangeId=Math.Max(_lastChangeId,batch.MaxId);
            foreach(var change in batch.Entries.Where(x=>!string.IsNullOrWhiteSpace(x.Message)).TakeLast(3))_notifications.Show("Сетевой планировщик",change.Message!);
            await RefreshAsync(ct);
        }
        catch(OperationCanceledException){}
        catch(Exception ex){StatusText=$"Синхронизация: {ex.Message}";}
    }

    private async Task NewTaskAsync(CancellationToken ct=default)
    {
        if(SelectedUser is null)return;
        var draft=_dialogs.ShowTaskEditor(null,null,AccessibleUsers.ToList(),SelectedUser.Id,LoggedUser,AnchorDate.Date.AddHours(9),SelectedNewTaskDurationMinutes);if(draft is null)return;
        try{await _tasks.CreateAsync(LoggedUser,draft,ct);if(!string.IsNullOrWhiteSpace(draft.CronSchedule))await _recurrence.ProcessAsync(ct);await RefreshAsync(ct);}catch(Exception ex){_notifications.Show("Ошибка создания",ex.Message);}
    }

    private async Task EditTaskAsync(CancellationToken ct=default)
    {
        if(SelectedTask is null){_notifications.Show("Сетевой планировщик","Выберите задачу в календаре или шаблон повторения.");return;}
        var rem=await _tasks.GetReminderOffsetAsync(SelectedTask.Id,ct);
        var draft=_dialogs.ShowTaskEditor(SelectedTask,rem,AccessibleUsers.ToList(),SelectedTask.AssignedToUserId,LoggedUser);if(draft is null)return;
        try{await _tasks.UpdateAsync(LoggedUser,draft,ct);await _recurrence.ProcessAsync(ct);await RefreshAsync(ct);}catch(Exception ex){_notifications.Show("Ошибка сохранения",ex.Message);}
    }

    private async Task NewUserAsync(CancellationToken ct=default)
    {
        var user=_dialogs.ShowUserEditor(null,AdminUsers.ToList());if(user is null)return;
        try{await _admin.SaveAsync(LoggedUser,user,ct);await ReloadUsersAsync(ct);await RefreshAsync(ct);}catch(Exception ex){_notifications.Show("Ошибка пользователя",ex.Message);}
    }

    private async Task EditUserAsync(CancellationToken ct=default)
    {
        if(SelectedAdminUser is null)return;var user=_dialogs.ShowUserEditor(SelectedAdminUser,AdminUsers.ToList());if(user is null)return;
        try{await _admin.SaveAsync(LoggedUser,user,ct);await ReloadUsersAsync(ct);await RefreshAsync(ct);}catch(Exception ex){_notifications.Show("Ошибка пользователя",ex.Message);}
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
