using System.Text.Json;
using System.Text.Json.Nodes;

namespace Planner.Data.Configuration;

/// <summary>
/// Хранилище настроек в двух слоях. Файл рядом с программой задаёт значения по умолчанию
/// для всего отдела, а личный файл в профиле пользователя перекрывает только изменённые
/// параметры. Программа установлена в папку, доступную лишь на чтение, поэтому окно
/// настроек всегда пишет в профиль.
/// </summary>
public sealed class SettingsStore
{
    public const string FileName = "planner.settings.json";

    public SettingsStore(string? applicationDirectory = null)
    {
        DefaultsPath = Path.Combine(applicationDirectory ?? AppContext.BaseDirectory, FileName);
        UserPath = Path.Combine(UserDataFolder, FileName);
    }

    /// <summary>Файл, поставляемый вместе с программой.</summary>
    public string DefaultsPath { get; }

    /// <summary>Личный файл пользователя, куда сохраняет окно настроек.</summary>
    public string UserPath { get; }

    public static string UserDataFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NetworkPlanner");

    public PlannerSettings Load()
    {
        var merged = new JsonObject();
        Overlay(merged, ReadObject(DefaultsPath));
        Overlay(merged, ReadObject(UserPath));

        if (merged.Count == 0)
            throw new FileNotFoundException(
                $"Не найден файл настроек: {DefaultsPath}");

        return merged.Deserialize<PlannerSettings>(PlannerSettings.JsonOptions)
               ?? throw new InvalidOperationException("Не удалось прочитать настройки приложения.");
    }

    public void Save(PlannerSettings settings)
    {
        Directory.CreateDirectory(UserDataFolder);
        var json = JsonSerializer.Serialize(settings, PlannerSettings.JsonOptions);
        // Запись через временный файл: сбой на середине не оставит пользователя без настроек.
        var temporary = UserPath + ".tmp";
        File.WriteAllText(temporary, json);
        File.Move(temporary, UserPath, true);
    }

    private static JsonObject? ReadObject(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            return JsonNode.Parse(File.ReadAllText(path)) as JsonObject;
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            // Испорченный или недоступный файл не должен мешать запуску: берётся другой слой.
            return null;
        }
    }

    private static void Overlay(JsonObject target, JsonObject? source)
    {
        if (source is null) return;
        foreach (var pair in source)
            target[pair.Key] = pair.Value?.DeepClone();
    }
}
