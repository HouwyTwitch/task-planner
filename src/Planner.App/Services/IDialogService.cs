using Planner.App.ViewModels;
using Planner.Core.Models;

namespace Planner.App.Services;

public interface IDialogService
{
    TaskDraft? ShowTaskEditor(TaskItem? existing,long? reminderOffset,IReadOnlyList<User> users,long defaultAssignee,User loggedUser,DateTime? defaultStart=null,int defaultDurationMinutes=60);
    User? ShowUserEditor(User? existing,IReadOnlyList<User> users);
    void ShowStatistics(StatisticsViewModel statistics);
    /// <summary>Окно настроек и свойств программы. Возвращает true, если настройки сохранены.</summary>
    bool ShowSettings(SettingsViewModel settings);
    bool Confirm(string title,string message);
    /// <summary>Вопрос с тремя ответами: «да», «нет» и «отменить действие».</summary>
    bool? ConfirmOrCancel(string title,string message);
    void Information(string title,string message);
    /// <summary>Ошибка операции показывается модальным окном: системные уведомления могут быть отключены политикой.</summary>
    void Error(string title,string message);
}

public interface INotificationService
{
    void Show(string title,string message);
}
