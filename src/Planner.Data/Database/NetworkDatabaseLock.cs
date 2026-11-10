using Planner.Data.Configuration;

namespace Planner.Data.Database;

public sealed class NetworkDatabaseLock
{
    private readonly PlannerSettings _settings;

    // Фоновые циклы (повторы, напоминания, синхронизация) и действия пользователя внутри
    // одного процесса раньше соревновались за файл блокировки: проигравший засыпал на
    // 50–150 мс и пробовал снова, иногда много раз подряд. Локальная очередь выстраивает
    // их друг за другом без лишних попыток открыть файл по сети.
    private readonly SemaphoreSlim _localGate = new(1, 1);

    public NetworkDatabaseLock(PlannerSettings settings) => _settings = settings;

    public async Task<NetworkLockLease> AcquireAsync(CancellationToken cancellationToken = default)
    {
        var timeout = TimeSpan.FromSeconds(_settings.DatabaseLockTimeoutSeconds);
        if (!await _localGate.WaitAsync(timeout, cancellationToken).ConfigureAwait(false))
            throw new TimeoutException("База занята другой операцией этой программы. Повторите операцию.");
        var deadline = DateTime.UtcNow + timeout;

        Exception? lastError = null;
        var delay = 20;
        try
        {
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    // Синхронное открытие: асинхронный FileStream в Windows добавляет
                    // лишний круг через пул ввода-вывода, а мы и так уже в фоновом потоке.
                    var stream = new FileStream(_settings.LockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 256, FileOptions.None);
                    try
                    {
                        stream.SetLength(0);
                        var payload = $"{Environment.MachineName};{Environment.UserName};{Environment.ProcessId};{DateTime.UtcNow:O}";
                        stream.Write(System.Text.Encoding.UTF8.GetBytes(payload));
                        stream.Flush();
                        stream.Position = 0;
                    }
                    catch
                    {
                        stream.Dispose();
                        throw;
                    }
                    return new NetworkLockLease(stream, _localGate);
                }
                catch (IOException ex)
                {
                    lastError = ex;
                    // Первые попытки частые: обычно чужая операция занимает десятки миллисекунд.
                    await Task.Delay(Random.Shared.Next(delay, delay * 2), cancellationToken).ConfigureAwait(false);
                    delay = Math.Min(delay * 2, 200);
                }
            }
        }
        catch
        {
            _localGate.Release();
            throw;
        }

        _localGate.Release();
        throw new TimeoutException("Сетевая база занята другим пользователем. Повторите операцию.", lastError);
    }
}

public sealed class NetworkLockLease : IAsyncDisposable, IDisposable
{
    private FileStream? _stream;
    private SemaphoreSlim? _gate;

    internal NetworkLockLease(FileStream stream, SemaphoreSlim gate)
    {
        _stream = stream;
        _gate = gate;
    }

    public void Dispose()
    {
        var stream = Interlocked.Exchange(ref _stream, null);
        if (stream is null) return;
        try { stream.Dispose(); }
        finally { Interlocked.Exchange(ref _gate, null)?.Release(); }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
