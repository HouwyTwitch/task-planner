using Planner.Data.Configuration;

namespace Planner.Data.Services;

public sealed class SignalWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly object _gate=new();
    private Timer? _debounce;
    public event EventHandler? SignalReceived;

    public SignalWatcher(PlannerSettings settings,long userId)
    {
        Directory.CreateDirectory(settings.SignalsFolder);
        _watcher=new FileSystemWatcher(settings.SignalsFolder)
        {
            Filter=$"{userId}.sig", NotifyFilter=NotifyFilters.LastWrite|NotifyFilters.CreationTime|NotifyFilters.FileName,
            EnableRaisingEvents=true
        };
        _watcher.Changed+=OnSignal; _watcher.Created+=OnSignal; _watcher.Renamed+=OnRenamed;
    }

    private void OnSignal(object sender,FileSystemEventArgs e)=>Debounce();
    private void OnRenamed(object sender,RenamedEventArgs e)=>Debounce();
    private void Debounce()
    {
        lock(_gate)
        {
            _debounce?.Dispose();
            _debounce=new Timer(_=>SignalReceived?.Invoke(this,EventArgs.Empty),null,500,Timeout.Infinite);
        }
    }
    public void Dispose(){_watcher.Dispose();lock(_gate){_debounce?.Dispose();_debounce=null;}}
}
