using System.Windows.Input;

namespace Planner.App.Infrastructure;

public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute; private readonly Func<object?,bool>? _can;
    public RelayCommand(Action execute,Func<bool>? can=null){_execute=_=>execute();_can=can is null?null:_=>can();}
    public RelayCommand(Action<object?> execute,Func<object?,bool>? can=null){_execute=execute;_can=can;}
    public bool CanExecute(object? p)=>_can?.Invoke(p)??true;
    public void Execute(object? p)=>_execute(p);
    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged()=>CanExecuteChanged?.Invoke(this,EventArgs.Empty);
}

public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<object?,Task> _execute; private readonly Func<object?,bool>? _can; private bool _running;
    public AsyncRelayCommand(Func<Task> execute,Func<bool>? can=null){_execute=_=>execute();_can=can is null?null:_=>can();}
    public AsyncRelayCommand(Func<object?,Task> execute,Func<object?,bool>? can=null){_execute=execute;_can=can;}
    public bool CanExecute(object? p)=>!_running&&(_can?.Invoke(p)??true);
    public async void Execute(object? p){if(!CanExecute(p))return;_running=true;Raise();try{await _execute(p);}finally{_running=false;Raise();}}
    public event EventHandler? CanExecuteChanged; public void Raise()=>CanExecuteChanged?.Invoke(this,EventArgs.Empty);
}
