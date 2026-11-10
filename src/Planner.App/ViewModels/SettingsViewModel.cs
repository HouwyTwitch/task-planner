using System.IO;
using Planner.App.Infrastructure;
using Planner.App.Services;
using Planner.Core.Models;
using Planner.Data.Configuration;

namespace Planner.App.ViewModels;

/// <summary>
/// Окно «Настройки и свойства программы»: сетевые пути, параметры календаря,
/// сведения о версии и проверка обновлений.
/// </summary>
public sealed class SettingsViewModel : ObservableObject
{
    private readonly SettingsStore _store;
    private readonly PlannerSettings _current;
    private AvailableUpdate? _available;

    public SettingsViewModel(SettingsStore store, PlannerSettings current)
    {
        _store = store;
        _current = current;

        _sharedFolder = current.SharedFolder;
        _databaseFileName = current.DatabaseFileName;
        _signalsFolderName = current.SignalsFolderName;
        _updateFolder = current.UpdateFolder;
        _updateManifestFileName = current.UpdateManifestFileName;
        _checkUpdatesOnStartup = current.CheckUpdatesOnStartup;
        _snapMinutes = current.SnapMinutes;
        _recurrenceHorizonMonths = current.RecurrenceHorizonMonths;

        CheckUpdatesCommand = new AsyncRelayCommand(CheckUpdatesAsync);
        InstallUpdateCommand = new AsyncRelayCommand(InstallUpdateAsync, () => _available is not null);
    }

    // --- Свойства программы ---
    public string ProductName => AppInfo.ProductName;
    public string VersionDisplay => AppInfo.VersionDisplay;
    public string BuildDateDisplay => AppInfo.BuildDateDisplay;
    public string InstallFolder => AppInfo.InstallFolder;
    public string SettingsFilePath => _store.UserPath;
    public string LogFilePath => AppInfo.LogFilePath;

    // --- Расположение данных ---
    private string _sharedFolder;
    public string SharedFolder { get => _sharedFolder; set => Set(ref _sharedFolder, value); }

    private string _databaseFileName;
    public string DatabaseFileName { get => _databaseFileName; set => Set(ref _databaseFileName, value); }

    private string _signalsFolderName;
    public string SignalsFolderName { get => _signalsFolderName; set => Set(ref _signalsFolderName, value); }

    // --- Обновления ---
    private string _updateFolder;
    public string UpdateFolder { get => _updateFolder; set => Set(ref _updateFolder, value); }

    private string _updateManifestFileName;
    public string UpdateManifestFileName { get => _updateManifestFileName; set => Set(ref _updateManifestFileName, value); }

    private bool _checkUpdatesOnStartup;
    public bool CheckUpdatesOnStartup { get => _checkUpdatesOnStartup; set => Set(ref _checkUpdatesOnStartup, value); }

    // --- Календарь ---
    private int _snapMinutes;
    public int SnapMinutes { get => _snapMinutes; set => Set(ref _snapMinutes, value); }

    private int _recurrenceHorizonMonths;
    public int RecurrenceHorizonMonths { get => _recurrenceHorizonMonths; set => Set(ref _recurrenceHorizonMonths, value); }

    private string _updateStatus = string.Empty;
    public string UpdateStatus { get => _updateStatus; private set => Set(ref _updateStatus, value); }

    public AsyncRelayCommand CheckUpdatesCommand { get; }
    public AsyncRelayCommand InstallUpdateCommand { get; }

    /// <summary>Перезапуск нужен, если изменилось расположение общей базы.</summary>
    public bool RequiresRestart =>
        !string.Equals(SharedFolder?.Trim(), _current.SharedFolder, StringComparison.OrdinalIgnoreCase) ||
        !string.Equals(DatabaseFileName?.Trim(), _current.DatabaseFileName, StringComparison.OrdinalIgnoreCase) ||
        !string.Equals(SignalsFolderName?.Trim(), _current.SignalsFolderName, StringComparison.OrdinalIgnoreCase);

    /// <summary>Проверяет введённые значения и сохраняет их в личный файл настроек.</summary>
    /// <exception cref="InvalidOperationException">Значения заполнены неверно.</exception>
    public PlannerSettings Save()
    {
        var updated = Build();
        updated.Validate();
        _store.Save(updated);

        // Работающие службы держат ссылку на текущий объект настроек, поэтому он обновляется на месте.
        Apply(updated, _current);
        return _current;
    }

    private PlannerSettings Build()
    {
        var draft = _current.Clone();
        draft.SharedFolder = (SharedFolder ?? string.Empty).Trim();
        draft.DatabaseFileName = (DatabaseFileName ?? string.Empty).Trim();
        draft.SignalsFolderName = (SignalsFolderName ?? string.Empty).Trim();
        draft.UpdateFolder = (UpdateFolder ?? string.Empty).Trim();
        draft.UpdateManifestFileName = string.IsNullOrWhiteSpace(UpdateManifestFileName) ? "update.json" : UpdateManifestFileName.Trim();
        draft.CheckUpdatesOnStartup = CheckUpdatesOnStartup;
        draft.SnapMinutes = SnapMinutes;
        draft.RecurrenceHorizonMonths = RecurrenceHorizonMonths;
        return draft;
    }

    private static void Apply(PlannerSettings source, PlannerSettings target)
    {
        target.SharedFolder = source.SharedFolder;
        target.DatabaseFileName = source.DatabaseFileName;
        target.SignalsFolderName = source.SignalsFolderName;
        target.UpdateFolder = source.UpdateFolder;
        target.UpdateManifestFileName = source.UpdateManifestFileName;
        target.CheckUpdatesOnStartup = source.CheckUpdatesOnStartup;
        target.SnapMinutes = source.SnapMinutes;
        target.RecurrenceHorizonMonths = source.RecurrenceHorizonMonths;
    }

    private async Task CheckUpdatesAsync()
    {
        _available = null;
        InstallUpdateCommand.Raise();

        var folder = (UpdateFolder ?? string.Empty).Trim();
        if (folder.Length == 0)
        {
            UpdateStatus = "Папка обновлений не указана.";
            return;
        }

        UpdateStatus = "Проверка…";
        try
        {
            // Проверка идёт по введённым в окне значениям, ещё до сохранения.
            var probe = _current.Clone();
            probe.UpdateFolder = folder;
            probe.UpdateManifestFileName = string.IsNullOrWhiteSpace(UpdateManifestFileName) ? "update.json" : UpdateManifestFileName.Trim();

            var service = new UpdateService(probe);
            _available = await service.CheckAsync(TimeSpan.FromSeconds(10));
            UpdateStatus = _available is null
                ? $"Установлена последняя версия {AppInfo.VersionDisplay}."
                : $"{UpdateService.DescribeUpdate(_available)}. Установлена {AppInfo.VersionDisplay}.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or TimeoutException or IOException or UnauthorizedAccessException)
        {
            UpdateStatus = ex.Message;
        }
        finally
        {
            InstallUpdateCommand.Raise();
        }
    }

    /// <summary>Установка обновления: подготовка и запрос на перезапуск.</summary>
    public event EventHandler<AvailableUpdate>? UpdateReadyToInstall;

    private async Task InstallUpdateAsync()
    {
        if (_available is null) return;
        var update = _available;
        try
        {
            UpdateStatus = "Подготовка обновления…";
            var progress = new Progress<string>(text => UpdateStatus = text);
            await new UpdateService(_current).PrepareAndLaunchAsync(update, progress);
            UpdateReadyToInstall?.Invoke(this, update);
        }
        catch (Exception ex)
        {
            UpdateStatus = $"Не удалось установить обновление: {ex.Message}";
        }
    }
}
