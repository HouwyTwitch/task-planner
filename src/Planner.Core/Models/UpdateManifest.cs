using System.Text.Json.Serialization;

namespace Planner.Core.Models;

/// <summary>
/// Небольшой файл-описание новой версии в сетевой папке обновлений.
/// Программа читает только его, поэтому проверка обновления занимает доли секунды
/// и не требует чтения самого архива.
/// </summary>
public sealed class UpdateManifest
{
    /// <summary>Версия в формате 1.2.3 или 1.2.3.4.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Имя ZIP-архива в той же папке. Допускается и относительный путь.</summary>
    public string File { get; set; } = string.Empty;

    /// <summary>Дата выпуска — показывается пользователю.</summary>
    public DateTime? ReleasedAt { get; set; }

    /// <summary>Список изменений для окна «Проверить обновления».</summary>
    public string? Notes { get; set; }

    [JsonIgnore]
    public bool IsValid => System.Version.TryParse(Version, out _) && !string.IsNullOrWhiteSpace(File);
}

/// <summary>Найденное обновление вместе с полным путём к архиву.</summary>
public sealed record AvailableUpdate(Version Version, string ArchivePath, DateTime? ReleasedAt, string? Notes)
{
    public string ReleasedAtDisplay => ReleasedAt is null ? string.Empty : ReleasedAt.Value.ToString("dd.MM.yyyy");
}
