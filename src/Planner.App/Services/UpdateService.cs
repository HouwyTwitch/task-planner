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
/// Установка: архив распаковывается во временную папку, файлы запущенной программы
/// переименовываются (Windows это разрешает), на их место копируются новые, после чего
/// запускается новая версия, а текущая завершается. Остатки старой версии новая
/// удаляет при запуске. Внешние сценарии не нужны.
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
    /// Первый шаг установки: копирует архив на локальный диск и распаковывает его.
    /// Установленная программа на этом шаге не затрагивается.
    /// </summary>
    public async Task<PreparedUpdate> PrepareAsync(AvailableUpdate update, IProgress<string>? progress = null, CancellationToken ct = default)
    {
        Log($"Подготовка версии {FormatVersion(update.Version)} из {update.ArchivePath}");
        progress?.Report("Копирование архива…");
        Directory.CreateDirectory(DownloadFolder);
        var localArchive = Path.Combine(DownloadFolder, Path.GetFileName(update.ArchivePath));
        await CopyFileAsync(update.ArchivePath, localArchive, ct);

        progress?.Report("Распаковка…");
        await Task.Run(() =>
        {
            ResetFolder(StagingFolder);
            ZipFile.ExtractToDirectory(localArchive, StagingFolder, overwriteFiles: true);
        }, ct);

        var source = ResolveArchiveRoot(StagingFolder);
        var executableName = Path.GetFileName(AppInfo.ExecutablePath);
        if (!File.Exists(Path.Combine(source, executableName)))
            throw new InvalidOperationException(
                $"В архиве обновления нет файла «{executableName}». Проверьте, что архив собран командой publish.ps1.");

        Log("Архив распакован");
        return new PreparedUpdate(update.Version, source);
    }

    /// <summary>
    /// Второй шаг: заменяет файлы установленной программы и запускает новую версию.
    /// После успешного вызова текущий процесс нужно сразу завершить.
    /// <para>
    /// Внешний сценарий-обновлятель больше не используется: в организациях запуск
    /// PowerShell-сценариев из профиля пользователя часто запрещён политикой, и тогда
    /// обновление молча не устанавливалось. Вместо этого программа пользуется тем, что
    /// Windows разрешает переименовывать файлы запущенной программы: старые файлы
    /// отодвигаются в сторону (*.updold), на их место копируются новые. Остатки старой
    /// версии удаляются при следующем запуске.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">Файлы заменить не удалось; установленная версия восстановлена.</exception>
    public static void ApplyAndRestart(PreparedUpdate prepared)
    {
        var target = AppInfo.InstallFolder;
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        var moved = new List<(string Original, string Backup)>();
        var copied = new List<string>();
        Log($"Установка версии {FormatVersion(prepared.Version)} в {target}");

        try
        {
            foreach (var file in Directory.EnumerateFiles(prepared.SourceFolder, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(prepared.SourceFolder, file);
                var destination = Path.Combine(target, relative);

                // planner.settings.json не перезаписывается: в нём пути, настроенные администратором.
                if (string.Equals(relative, SettingsFileName, StringComparison.OrdinalIgnoreCase) && File.Exists(destination))
                    continue;

                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                if (File.Exists(destination))
                {
                    var backup = $"{destination}.{stamp}{BackupExtension}";
                    File.Move(destination, backup);
                    moved.Add((destination, backup));
                }
                File.Copy(file, destination, overwrite: false);
                copied.Add(destination);
            }
        }
        catch (Exception ex)
        {
            Log($"Ошибка замены файлов: {ex}");
            Rollback(copied, moved);
            var hint = ex is UnauthorizedAccessException
                ? $" Нет прав на запись в папку программы «{target}». Установите обновление от имени пользователя с правами на эту папку или перенесите программу в папку, доступную для записи."
                : string.Empty;
            throw new InvalidOperationException($"Не удалось заменить файлы программы: {ex.Message}.{hint} Подробности: {UpdateLogPath}", ex);
        }

        Log($"Заменено файлов: {copied.Count}");
        try
        {
            // Новая версия дождётся выхода этого процесса и уберёт остатки старой.
            Process.Start(new ProcessStartInfo(AppInfo.ExecutablePath)
            {
                Arguments = $"{AfterUpdateArgument} {Environment.ProcessId.ToString(CultureInfo.InvariantCulture)}",
                WorkingDirectory = target,
                UseShellExecute = true
            });
            Log("Новая версия запущена");
        }
        catch (Exception ex)
        {
            // Файлы уже новые: достаточно открыть программу вручную.
            Log($"Не удалось запустить новую версию: {ex}");
            throw new InvalidOperationException($"Обновление установлено, но программу не удалось запустить автоматически: {ex.Message}. Откройте её вручную.", ex);
        }
    }

    public const string AfterUpdateArgument = "--after-update";
    private const string BackupExtension = ".updold";
    private const string SettingsFileName = "planner.settings.json";

    /// <summary>
    /// Вызывается при запуске. Если программу запустил установщик обновления — ждёт
    /// выхода прежнего процесса. Затем удаляет файлы, оставшиеся от прежней версии.
    /// </summary>
    public static void CompletePendingUpdate(string[] args)
    {
        var index = Array.IndexOf(args, AfterUpdateArgument);
        if (index >= 0 && index + 1 < args.Length && int.TryParse(args[index + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pid))
        {
            try
            {
                using var previous = Process.GetProcessById(pid);
                previous.WaitForExit(30_000);
            }
            catch (ArgumentException) { }          // прежний процесс уже завершился
            catch (InvalidOperationException) { }
            Log($"Запущена версия {AppInfo.VersionDisplay}");
        }

        _ = Task.Run(CleanupLeftovers);
    }

    private static void CleanupLeftovers()
    {
        try
        {
            foreach (var file in Directory.EnumerateFiles(AppInfo.InstallFolder, "*" + BackupExtension, SearchOption.AllDirectories))
            {
                // Файл ещё может быть занят завершающимся процессом — тогда уберём его в следующий раз.
                try { File.Delete(file); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
            if (Directory.Exists(StagingFolder)) Directory.Delete(StagingFolder, true);
            if (Directory.Exists(DownloadFolder)) Directory.Delete(DownloadFolder, true);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private static void Rollback(List<string> copied, List<(string Original, string Backup)> moved)
    {
        foreach (var file in copied)
        {
            try { File.Delete(file); } catch (Exception ex) { Log($"Откат: не удалось удалить {file}: {ex.Message}"); }
        }
        for (var i = moved.Count - 1; i >= 0; i--)
        {
            try { File.Move(moved[i].Backup, moved[i].Original, overwrite: true); }
            catch (Exception ex) { Log($"Откат: не удалось вернуть {moved[i].Original}: {ex.Message}"); }
        }
        Log("Установленная версия восстановлена");
    }

    private static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(UpdateFolder);
            File.AppendAllText(UpdateLogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}", Encoding.UTF8);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
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

/// <summary>Распакованное обновление, готовое к установке.</summary>
public sealed record PreparedUpdate(Version Version, string SourceFolder);
