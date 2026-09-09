using Planner.App.ViewModels;
using Planner.Core.Models;

namespace Planner.App.Services;

public interface IDialogService
{
    TaskDraft? ShowTaskEditor(TaskItem? existing,long? reminderOffset,IReadOnlyList<User> users,long defaultAssignee,User loggedUser,DateTime? defaultStart=null,int defaultDurationMinutes=60);
    User? ShowUserEditor(User? existing,IReadOnlyList<User> users);
    void ShowStatistics(StatisticsViewModel statistics);
    bool Confirm(string title,string message);
    /// <summary>Ошибка операции показывается модальным окном: системные уведомления могут быть отключены политикой.</summary>
    void Error(string title,string message);
}

public interface INotificationService
{
    void Show(string title,string message);
}
