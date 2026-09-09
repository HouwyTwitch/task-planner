using Planner.App.ViewModels;
using Planner.Data.Configuration;
using Planner.Data.Repositories;
using Planner.Data.Services;

namespace Planner.App.Services;

public sealed class BackgroundCoordinator : IDisposable
{
    private readonly PlannerSettings _settings; private readonly long _userId; private readonly RecurringTaskProcessor _recurrence; private readonly ReminderRepository _reminders;
    private readonly MainViewModel _vm; private readonly INotificationService _notifications; private readonly HashSet<string> _triggered=new(); private readonly List<Task> _loops=new();
    private CancellationTokenSource? _linked;
    public BackgroundCoordinator(PlannerSettings settings,long userId,RecurringTaskProcessor recurrence,ReminderRepository reminders,MainViewModel vm,INotificationService notifications)
    {_settings=settings;_userId=userId;_recurrence=recurrence;_reminders=reminders;_vm=vm;_notifications=notifications;}

    public void Start(CancellationToken appToken)
    {
        _linked=CancellationTokenSource.CreateLinkedTokenSource(appToken);
        _loops.Add(Task.Run(()=>CronLoop(_linked.Token),_linked.Token));
        _loops.Add(Task.Run(()=>ReminderLoop(_linked.Token),_linked.Token));
        _loops.Add(Task.Run(()=>SyncLoop(_linked.Token),_linked.Token));
    }
    private async Task CronLoop(CancellationToken ct)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(30,_settings.CronPollSeconds)));
        while(await timer.WaitForNextTickAsync(ct)) try{await _recurrence.ProcessAsync(ct);}catch(OperationCanceledException){}catch{}
    }
    private async Task ReminderLoop(CancellationToken ct)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(10,_settings.ReminderPollSeconds)));
        while(await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                var now=DateTime.Now;
                var list=await _reminders.GetCandidateRemindersAsync(_userId,now.AddSeconds(-_settings.ReminderPollSeconds*2),now.AddSeconds(2),ct);
                foreach(var r in list)
                {
                    var key=$"{r.ReminderId}:{r.TaskStart:O}";
                    lock(_triggered)
                    {
                        // Набор показанных напоминаний живёт весь сеанс, поэтому его надо ограничивать.
                        if(_triggered.Count>1000) _triggered.Clear();
                        if(!_triggered.Add(key)) continue;
                    }
                    _notifications.Show("Напоминание",$"{r.TaskTitle}\nНачало: {r.TaskStart:g}");
                }
            }catch(OperationCanceledException){}catch{}
        }
    }
    private async Task SyncLoop(CancellationToken ct)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(10,_settings.FallbackSyncSeconds)));
        while(await timer.WaitForNextTickAsync(ct)) try{await System.Windows.Application.Current.Dispatcher.InvokeAsync(()=>_vm.HandleExternalSignalAsync(ct)).Task.Unwrap();}catch(OperationCanceledException){}catch{}
    }
    public void Dispose(){_linked?.Cancel();_linked?.Dispose();}
}
