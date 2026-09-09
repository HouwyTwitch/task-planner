using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Planner.Core.Models;
using Planner.Data.Configuration;

namespace Planner.App.Services;

/// <summary>
/// Обновление программы из ZIP-архива в сетевой папке.
///
/// Проверка намеренно сделана дешёвой: читается только небольшой файл-манифест
/// <c>update.json</c>, а сам архив не открывается. Если манифеста нет, разбираются
/// имена ZIP-файлов вида «Планировщик-1.2.0.zip» — это тоже лишь перечисление каталога.
///
/// Установку выполняет отдельный сценарий-обновлятель: программа не может заменить
/// собственные файлы, пока работает, поэтому она распаковывает архив во временную папку,
/// запускает сценарий и завершается. Сценарий дожидается выхода программы, копирует
/// файлы поверх установленной версии и запускает её заново.
/// </summary>
public sealed class UpdateService
{
    private static readonly Regex VersionInName = new(@"(\d+\.\d+(?:\.\d+){0,2})", RegexOptions.Compiled);

    private readonly PlannerSettings _settings;

    public UpdateService(PlannerSettings settings) => _settings = settings;

    public static string UpdateFolder => Path.Combine(SettingsStore.UserDataFolder, "update");
    public static string UpdateLogPath => Path.Combine(UpdateFolder, "update.log");
    private static string StagingFolder => Path.Combine(UpdateFolder, "staging");
    private static string DownloadFolder => Path.Combine(UpdateFolder, "download");
    private static string ScriptPath => Path.Combine(UpdateFolder, "apply-update.ps1");

    public Version CurrentVersion => Normalize(AppInfo.Version);

    /// <summary>
    /// Ищет более новую версию. Возвращает <c>null</c>, если обновления нет,
    /// папка не настроена или сетевая папка недоступна.
    /// </summary>
    /// <exception cref="InvalidOperationException">Папка обновлений настроена, но данные в ней некорректны.</exception>
    public async Task<AvailableUpdate?> CheckAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        if (!_settings.HasUpdateFolder) return null;

        // Обращение к недоступной сетевой папке может подвиснуть, поэтому у проверки есть срок.
        var probe = Task.Run(ReadAvailableUpdate, CancellationToken.None);
        var finished = await Task.WhenAny(probe, Task.Delay(timeout, ct));
        if (finished != probe)
        {
            // Брошенная проверка не должна всплыть как необработанная ошибка фоновой задачи.
            _ = probe.ContinueWith(static t => _ = t.Exception, TaskScheduler.Default);
            ct.ThrowIfCancellationRequested();
            throw new TimeoutException($"Папка обновлений не отвечает: {_settings.UpdateFolder}");
        }

        return await probe;
    }

    private AvailableUpdate? ReadAvailableUpdate()
    {
        var found = ReadManifest() ?? FindNewestArchive();
        if (found is null) return null;
        return Normalize(found.Version) > CurrentVersion ? found : null;
    }

    private AvailableUpdate? ReadManifest()
    {
        var path = _settings.UpdateManifestPath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;

        UpdateManifest? manifest;
        try { manifest = JsonSerializer.Deserialize<UpdateManifest>(File.ReadAllText(path), PlannerSettings.JsonOptions); }
        catch (JsonException ex) { throw new InvalidOperationException($"Файл {path} повреждён: {ex.Message}", ex); }

        if (manifest is null || !manifest.IsValid)
            throw new InvalidOperationException($"В файле {path} не указаны корректные Version и File.");

        var archive = Path.IsPathRooted(manifest.File)
            ? manifest.File
            : Path.Combine(_settings.UpdateFolder, manifest.File);

        if (!File.Exists(archive))
            throw new InvalidOperationException($"Архив обновления не найден: {archive}");

        return new AvailableUpdate(Version.Parse(manifest.Version), archive, manifest.ReleasedAt, manifest.Notes);
    }

    /// <summary>Запасной путь без манифеста: самый новый по версии ZIP-архив в папке.</summary>
    private AvailableUpdate? FindNewestArchive()
    {
        if (!Directory.Exists(_settings.UpdateFolder)) return null;

        AvailableUpdate? best = null;
        foreach (var file in Directory.EnumerateFiles(_settings.UpdateFolder, "*.zip", SearchOption.TopDirectoryOnly))
        {
            var match = VersionInName.Match(Path.GetFileNameWithoutExtension(file));
            if (!match.Success || !Version.TryParse(match.Groups[1].Value, out var version)) continue;
            if (best is null || version > best.Version)
                best = new AvailableUpdate(version, file, File.GetLastWriteTime(file), null);
        }
        return best;
    }

    /// <summary>
    /// Готовит обновление и запускает сценарий установки. После успешного вызова
    /// программу нужно немедленно закрыть: файлы будут заменены сразу после её выхода.
    /// </summary>
    public async Task PrepareAndLaunchAsync(AvailableUpdate update, IProgress<string>? progress = null, CancellationToken ct = default)
    {
        progress?.Report("Копирование архива…");
        Directory.CreateDirectory(DownloadFolder);
        var localArchive = Path.Combine(DownloadFolder, Path.GetFileName(update.ArchivePath));
        await CopyFileAsync(update.ArchivePath, localArchive, ct);

        progress?.Report("Распаковка…");
        ResetFolder(StagingFolder);
        await Task.Run(() => ZipFile.ExtractToDirectory(localArchive, StagingFolder, overwriteFiles: true), ct);

        var source = ResolveArchiveRoot(StagingFolder);
        var executableName = Path.GetFileName(AppInfo.ExecutablePath);
        if (!File.Exists(Path.Combine(source, executableName)))
            throw new InvalidOperationException(
                $"В архиве обновления нет файла «{executableName}». Проверьте, что архив собран командой publish.ps1.");

        progress?.Report("Запуск установки…");
        WriteScript(source);
        Process.Start(new ProcessStartInfo("powershell.exe")
        {
            Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -WindowStyle Hidden -File \"{ScriptPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });
    }

    /// <summary>Архив может содержать файлы в корне либо внутри единственной папки.</summary>
    private static string ResolveArchiveRoot(string staging)
    {
        var files = Directory.GetFiles(staging);
        var folders = Directory.GetDirectories(staging);
        return files.Length == 0 && folders.Length == 1 ? folders[0] : staging;
    }

    private static void ResetFolder(string path)
    {
        if (Directory.Exists(path)) Directory.Delete(path, true);
        Directory.CreateDirectory(path);
    }

    private static async Task CopyFileAsync(string source, string destination, CancellationToken ct)
    {
        await using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        await using var output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await input.CopyToAsync(output, ct);
    }

    private static void WriteScript(string source)
    {
        var script = ScriptTemplate
            .Replace("%LOG%", Quote(UpdateLogPath), StringComparison.Ordinal)
            .Replace("%PID%", Environment.ProcessId.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("%SOURCE%", Quote(source), StringComparison.Ordinal)
            .Replace("%TARGET%", Quote(AppInfo.InstallFolder), StringComparison.Ordinal)
            .Replace("%EXE%", Quote(AppInfo.ExecutablePath), StringComparison.Ordinal)
            .Replace("%STAGING%", Quote(StagingFolder), StringComparison.Ordinal);

        Directory.CreateDirectory(UpdateFolder);
        // Windows PowerShell 5.1 распознаёт кириллицу в сценарии только при наличии BOM.
        File.WriteAllText(ScriptPath, script, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    /// <summary>
    /// Сценарий-обновлятель. Подстановки помечены знаками процента, а не интерполяцией:
    /// в тексте много фигурных скобок PowerShell, и подстановка по меткам исключает путаницу.
    /// </summary>
    private const string ScriptTemplate = """
$ErrorActionPreference = 'Continue'
$log = %LOG%
function Write-Log($message) {
    "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') $message" | Out-File -FilePath $log -Append -Encoding utf8
}

Write-Log 'Установка обновления начата'
try {
    Wait-Process -Id %PID% -Timeout 120 -ErrorAction SilentlyContinue
} catch { }
Start-Sleep -Seconds 1

# planner.settings.json не перезаписывается: в нём хранятся пути, настроенные администратором.
robocopy %SOURCE% %TARGET% /E /R:60 /W:1 /XF planner.settings.json /NFL /NDL /NJH /NJS | Out-Null
$code = $LASTEXITCODE
Write-Log "robocopy завершился с кодом $code"

if ($code -ge 8) {
    Write-Log 'Обновление не установлено: файлы скопировать не удалось'
    Add-Type -AssemblyName PresentationFramework
    [System.Windows.MessageBox]::Show("Не удалось установить обновление. Подробности: $log", 'Сетевой планировщик') | Out-Null
} else {
    Write-Log 'Файлы обновлены'
}

Start-Process -FilePath %EXE% -WorkingDirectory %TARGET%
Write-Log 'Программа запущена'
Remove-Item -LiteralPath %STAGING% -Recurse -Force -ErrorAction SilentlyContinue

""";

    /// <summary>Путь в одинарных кавычках PowerShell; завершающий разделитель ломает robocopy.</summary>
    private static string Quote(string path) =>
        "'" + path.TrimEnd(Path.DirectorySeparatorChar).Replace("'", "''", StringComparison.Ordinal) + "'";

    /// <summary>Приводит версию к четырём составляющим: «1.2» и «1.2.0.0» должны сравниваться одинаково.</summary>
    private static Version Normalize(Version version) =>
        new(version.Major, version.Minor, Math.Max(0, version.Build), Math.Max(0, version.Revision));

    public static string FormatVersion(Version version) => Normalize(version).ToString(3);

    public static string DescribeUpdate(AvailableUpdate update)
    {
        var text = $"Доступна версия {FormatVersion(update.Version)}";
        return update.ReleasedAt is null ? text : $"{text} от {update.ReleasedAtDisplay}";
    }
}
