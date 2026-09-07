using Planner.Core.Models;

namespace Planner.App.Services;

public interface IDialogService
{
    TaskDraft? ShowTaskEditor(TaskItem? existing,long? reminderOffset,IReadOnlyList<User> users,long defaultAssignee,User loggedUser,DateTime? defaultStart=null,int defaultDurationMinutes=60);
    User? ShowUserEditor(User? existing,IReadOnlyList<User> users);
    bool Confirm(string title,string message);
}

public interface INotificationService
{
    void Show(string title,string message);
}
