using System.IO;
using System.Reflection;
using Planner.Data.Configuration;

namespace Planner.App.Services;

/// <summary>Сведения о самой программе для окна свойств и для проверки обновлений.</summary>
public static class AppInfo
{
    private static readonly Assembly Entry = Assembly.GetExecutingAssembly();

    public static string ProductName =>
        Entry.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "Сетевой планировщик";

    /// <summary>Версия сборки без служебного четвёртого нуля: «1.1.0».</summary>
    public static Version Version
    {
        get
        {
            var version = Entry.GetName().Version ?? new Version(1, 0, 0);
            return new Version(version.Major, version.Minor, version.Build < 0 ? 0 : version.Build);
        }
    }

    public static string VersionDisplay => Version.ToString(3);

    /// <summary>Путь к запущенному файлу программы.</summary>
    public static string ExecutablePath =>
        Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "Сетевой планировщик.exe");

    public static string InstallFolder => AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

    /// <summary>Дата сборки берётся из файла программы: отдельного штампа версии не требуется.</summary>
    public static DateTime BuildDate
    {
        get
        {
            try { return File.GetLastWriteTime(ExecutablePath); }
            catch (IOException) { return DateTime.MinValue; }
        }
    }

    public static string BuildDateDisplay =>
        BuildDate == DateTime.MinValue ? "—" : BuildDate.ToString("dd.MM.yyyy HH:mm");

    public static string LogFilePath => Path.Combine(SettingsStore.UserDataFolder, "planner.log");
}
