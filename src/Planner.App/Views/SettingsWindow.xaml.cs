using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Planner.App.ViewModels;

namespace Planner.App.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is SettingsViewModel vm) vm.UpdateReadyToInstall += OnUpdateReady;
        };
        Closed += (_, _) =>
        {
            if (DataContext is SettingsViewModel vm) vm.UpdateReadyToInstall -= OnUpdateReady;
        };
    }

    /// <summary>Файлы уже подготовлены — программу нужно закрыть, чтобы сценарий их заменил.</summary>
    private void OnUpdateReady(object? sender, Services.PreparedUpdate update)
    {
        var answer = MessageBox.Show(this,
            $"Версия {Services.UpdateService.FormatVersion(update.Version)} готова к установке.\n" +
            "Программа закроется и сразу запустится заново уже обновлённой.",
            "Обновление", MessageBoxButton.OKCancel, MessageBoxImage.Information);
        if (answer != MessageBoxResult.OK) return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Services.UpdateService.ApplyAndRestart(update);
        }
        catch (Exception ex)
        {
            Mouse.OverrideCursor = null;
            MessageBox.Show(this, ex.Message, "Обновление", MessageBoxButton.OK, MessageBoxImage.Warning);
            if (DataContext is SettingsViewModel vm) vm.ReportUpdateFailure(ex.Message);
            return;
        }

        // Файлы уже заменены и новая версия запущена — текущую закрываем без промедления.
        Mouse.OverrideCursor = null;
        DialogResult = false;
        Application.Current.Shutdown();

        // Если завершение почему-либо зависнет (например, на системных уведомлениях),
        // процесс всё равно закрывается: новая версия уже запущена и ждёт его выхода.
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            Environment.Exit(0);
        });
    }

    private void BrowseShared_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && PickFolder(vm.SharedFolder) is string folder) vm.SharedFolder = folder;
    }

    private void BrowseUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && PickFolder(vm.UpdateFolder) is string folder) vm.UpdateFolder = folder;
    }

    private string? PickFolder(string? current)
    {
        var dialog = new OpenFolderDialog { Title = "Выберите папку", Multiselect = false };
        if (!string.IsNullOrWhiteSpace(current))
        {
            try { dialog.InitialDirectory = current; } catch (ArgumentException) { }
        }
        return dialog.ShowDialog(this) == true ? dialog.FolderName : null;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel vm) return;
        try
        {
            var restart = vm.RequiresRestart;
            vm.Save();
            if (restart)
                MessageBox.Show(this,
                    "Настройки сохранены. Расположение базы изменилось — закройте и снова откройте программу.",
                    "Настройки", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Проверка настроек", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
