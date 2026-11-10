using System.Windows;
using Planner.App.ViewModels;

namespace Planner.App.Views;

public partial class TaskEditorWindow : Window
{
    public TaskEditorWindow() => InitializeComponent();

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not TaskEditorViewModel vm) return;
        try
        {
            if (!ConfirmWeekendDate(vm)) return;
            _ = vm.BuildDraft();
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// Задача на субботу или воскресенье почти всегда выставлена по недосмотру,
    /// поэтому программа предлагает ближайший будний день. От переноса можно отказаться.
    /// </summary>
    private bool ConfirmWeekendDate(TaskEditorViewModel vm)
    {
        if (vm.GetWeekendSuggestion() is not DateTime suggestion) return true;

        var current = vm.Date!.Value.Date;
        var answer = MessageBox.Show(
            this,
            $"Задача назначена на выходной день — {current:dddd, dd.MM.yyyy}.\n\n" +
            $"Перенести её на ближайший будний день — {suggestion:dddd, dd.MM.yyyy}?\n\n" +
            "«Нет» — оставить выходной день. «Отмена» — вернуться к редактированию.",
            "Выходной день",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question);

        switch (answer)
        {
            case MessageBoxResult.Yes:
                vm.AcceptWeekendSuggestion(suggestion);
                return true;
            case MessageBoxResult.No:
                return true;
            default:
                return false;
        }
    }
}
