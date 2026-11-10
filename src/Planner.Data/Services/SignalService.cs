using Planner.Data.Configuration;

namespace Planner.Data.Services;

public sealed class SignalService
{
    private readonly PlannerSettings _settings;
    public SignalService(PlannerSettings settings) => _settings = settings;

    /// <summary>
    /// Записывает файл-сигнал получателю. Работа с сетевой папкой идёт в пуле потоков,
    /// чтобы медленный файловый сервер не задерживал поток интерфейса.
    /// </summary>
    public Task SignalUserAsync(long userId, CancellationToken ct = default) =>
        Task.Run(async () =>
        {
            Directory.CreateDirectory(_settings.SignalsFolder);
            var path = Path.Combine(_settings.SignalsFolder, $"{userId}.sig");
            var text = $"{DateTime.UtcNow:O};{Guid.NewGuid():N}";
            for (var i = 0; i < 3; i++)
            {
                try { File.WriteAllText(path, text); return; }
                catch (IOException) when (i < 2) { await Task.Delay(100 + i * 100, ct).ConfigureAwait(false); }
            }
        }, ct);
}
