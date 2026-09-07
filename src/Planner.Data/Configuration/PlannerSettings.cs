using System.Text.Json;

namespace Planner.Data.Configuration;

public sealed class PlannerSettings
{
    public string SharedFolder { get; set; } = @"\\server\share\Planner";
    public string DatabaseFileName { get; set; } = "planner.db";
    public string LockFileName { get; set; } = "planner.lock";
    public string SignalsFolderName { get; set; } = "Signals";
    public string TimeZoneId { get; set; } = string.Empty;
    public int DatabaseLockTimeoutSeconds { get; set; } = 10;
    public int FallbackSyncSeconds { get; set; } = 30;
    public int CronPollSeconds { get; set; } = 60;
    public int ReminderPollSeconds { get; set; } = 30;
    public int SnapMinutes { get; set; } = 30;
    public int RecurrenceHorizonMonths { get; set; } = 12;

    public string DatabasePath => Path.Combine(SharedFolder, DatabaseFileName);
    public string LockFilePath => Path.Combine(SharedFolder, LockFileName);
    public string SignalsFolder => Path.Combine(SharedFolder, SignalsFolderName);


    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SharedFolder))
            throw new InvalidOperationException("В planner.settings.json не указан SharedFolder.");

        if (!SharedFolder.StartsWith(@"\\", StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"SharedFolder должен быть UNC-путём сетевой папки, например \\\\fileserver\\Planner. Текущее значение: '{SharedFolder}'.");

        if (string.IsNullOrWhiteSpace(DatabaseFileName))
            throw new InvalidOperationException("DatabaseFileName не может быть пустым.");
        if (string.IsNullOrWhiteSpace(LockFileName))
            throw new InvalidOperationException("LockFileName не может быть пустым.");
        if (string.IsNullOrWhiteSpace(SignalsFolderName))
            throw new InvalidOperationException("SignalsFolderName не может быть пустым.");
        if (DatabaseLockTimeoutSeconds <= 0)
            throw new InvalidOperationException("DatabaseLockTimeoutSeconds должен быть больше 0.");
        if (FallbackSyncSeconds <= 0 || CronPollSeconds <= 0 || ReminderPollSeconds <= 0)
            throw new InvalidOperationException("Интервалы фоновых проверок должны быть больше 0.");
        if (SnapMinutes <= 0 || 60 % SnapMinutes != 0)
            throw new InvalidOperationException("SnapMinutes должен быть положительным делителем 60 (например 15 или 30).");
        if (RecurrenceHorizonMonths < 1 || RecurrenceHorizonMonths > 60)
            throw new InvalidOperationException("RecurrenceHorizonMonths должен быть от 1 до 60.");
    }

    public TimeZoneInfo ResolveTimeZone()
    {
        if (string.IsNullOrWhiteSpace(TimeZoneId)) return TimeZoneInfo.Local;
        return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
    }

    public static PlannerSettings Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Не найден файл настроек: {path}");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<PlannerSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Не удалось прочитать настройки приложения.");
    }
}
