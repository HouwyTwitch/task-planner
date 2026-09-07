using Planner.Data.Configuration;

namespace Planner.Data.Services;

public sealed class SignalService
{
    private readonly PlannerSettings _settings;
    public SignalService(PlannerSettings settings) => _settings = settings;

    public async Task SignalUserAsync(long userId, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_settings.SignalsFolder);
        var path = Path.Combine(_settings.SignalsFolder, $"{userId}.sig");
        var text = $"{DateTime.UtcNow:O};{Guid.NewGuid():N}";
        for (var i = 0; i < 3; i++)
        {
            try { await File.WriteAllTextAsync(path, text, ct); return; }
            catch (IOException) when (i < 2) { await Task.Delay(100 + i * 100, ct); }
        }
    }
}
