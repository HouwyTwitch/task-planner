using System.Text.Json;
using System.Text.Json.Serialization;

namespace Planner.Data.Configuration;

public sealed class PlannerSettings
{
    public const string DefaultSharedFolderPlaceholder = @"\\server\share\Planner";

    public string SharedFolder { get; set; } = DefaultSharedFolderPlaceholder;
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

    /// <summary>Сетевая папка с ZIP-архивами новых версий и файлом-манифестом.</summary>
    public string UpdateFolder { get; set; } = string.Empty;

    /// <summary>Имя манифеста в папке обновлений. Читается целиком, поэтому проверка мгновенная.</summary>
    public string UpdateManifestFileName { get; set; } = "update.json";

    /// <summary>Проверять обновление при запуске программы в фоне.</summary>
    public bool CheckUpdatesOnStartup { get; set; } = true;

    /// <summary>Высота одного часа в сетке календаря; меняется колесом мыши с Ctrl.</summary>
    public double CalendarHourHeight { get; set; } = 48;

    [JsonIgnore] public string DatabasePath => Path.Combine(SharedFolder, DatabaseFileName);
    [JsonIgnore] public string LockFilePath => Path.Combine(SharedFolder, LockFileName);
    [JsonIgnore] public string SignalsFolder => Path.Combine(SharedFolder, SignalsFolderName);
    [JsonIgnore] public bool HasUpdateFolder => !string.IsNullOrWhiteSpace(UpdateFolder);

    [JsonIgnore]
    public string UpdateManifestPath =>
        HasUpdateFolder ? Path.Combine(UpdateFolder, string.IsNullOrWhiteSpace(UpdateManifestFileName) ? "update.json" : UpdateManifestFileName) : string.Empty;

    /// <summary>Путь к папке базы ещё не настроен и остался значением из поставки.</summary>
    [JsonIgnore]
    public bool IsSharedFolderPlaceholder =>
        string.IsNullOrWhiteSpace(SharedFolder) ||
        SharedFolder.Contains(@"\\server\share", StringComparison.OrdinalIgnoreCase);

    public PlannerSettings Clone() => (PlannerSettings)MemberwiseClone();

    public void Validate()
    {
        var error = Check();
        if (error is not null) throw new InvalidOperationException(error);
    }

    /// <summary>Возвращает текст ошибки или <c>null</c>. Используется и при запуске, и в окне настроек.</summary>
    public string? Check()
    {
        if (string.IsNullOrWhiteSpace(SharedFolder))
            return "Не указана сетевая папка с базой данных.";

        if (IsSharedFolderPlaceholder)
            return @"Укажите реальный путь к сетевой папке с базой, например \\fileserver\Planner$\Planner.";

        if (!SharedFolder.StartsWith(@"\\", StringComparison.Ordinal) && !Path.IsPathRooted(SharedFolder))
            return $"Путь к папке с базой должен быть полным (UNC-путём сетевой папки либо путём с буквой диска). Текущее значение: «{SharedFolder}».";

        if (string.IsNullOrWhiteSpace(DatabaseFileName)) return "Имя файла базы не может быть пустым.";
        if (string.IsNullOrWhiteSpace(LockFileName)) return "Имя файла блокировки не может быть пустым.";
        if (string.IsNullOrWhiteSpace(SignalsFolderName)) return "Имя папки сигналов не может быть пустым.";
        if (DatabaseLockTimeoutSeconds <= 0) return "Ожидание блокировки базы должно быть больше 0 секунд.";
        if (FallbackSyncSeconds <= 0 || CronPollSeconds <= 0 || ReminderPollSeconds <= 0)
            return "Интервалы фоновых проверок должны быть больше 0 секунд.";
        if (SnapMinutes <= 0 || 60 % SnapMinutes != 0)
            return "Шаг сетки должен быть положительным делителем 60 — например 5, 10, 15 или 30 минут.";
        if (RecurrenceHorizonMonths is < 1 or > 60)
            return "Горизонт создания повторов должен быть от 1 до 60 месяцев.";
        if (HasUpdateFolder && !UpdateFolder.StartsWith(@"\\", StringComparison.Ordinal) && !Path.IsPathRooted(UpdateFolder))
            return $"Путь к папке обновлений должен быть полным. Текущее значение: «{UpdateFolder}».";
        return null;
    }

    public TimeZoneInfo ResolveTimeZone()
    {
        if (string.IsNullOrWhiteSpace(TimeZoneId)) return TimeZoneInfo.Local;
        return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
    }

    /// <summary>Общие правила чтения и записи JSON для настроек и манифеста обновления.</summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };
}
