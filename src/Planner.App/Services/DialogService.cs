using System.Windows;
using Planner.App.ViewModels;
using Planner.App.Views;
using Planner.Core.Models;

namespace Planner.App.Services;

public sealed class DialogService : IDialogService
{
    public TaskDraft? ShowTaskEditor(TaskItem? existing,long? reminderOffset,IReadOnlyList<User> users,long defaultAssignee,User loggedUser,DateTime? defaultStart=null,int defaultDurationMinutes=60)
    {
        var vm=new TaskEditorViewModel(existing,reminderOffset,users,defaultAssignee,loggedUser,defaultStart,defaultDurationMinutes);
        var w=new TaskEditorWindow{DataContext=vm,Owner=Application.Current.MainWindow};
        return w.ShowDialog()==true?vm.BuildDraft():null;
    }

    public User? ShowUserEditor(User? existing,IReadOnlyList<User> users)
    {
        var vm=new UserEditorViewModel(existing,users);
        var w=new UserEditorWindow{DataContext=vm,Owner=Application.Current.MainWindow};
        return w.ShowDialog()==true?vm.BuildUser():null;
    }

    public void ShowStatistics(StatisticsViewModel statistics)
    {
        var w=new StatisticsWindow{DataContext=statistics,Owner=Application.Current.MainWindow};
        w.ShowDialog();
    }

    public bool Confirm(string title,string message) =>
        MessageBox.Show(Application.Current.MainWindow,message,title,MessageBoxButton.YesNo,MessageBoxImage.Question)==MessageBoxResult.Yes;

    public void Error(string title,string message) =>
        MessageBox.Show(Application.Current.MainWindow,message,title,MessageBoxButton.OK,MessageBoxImage.Warning);
}
