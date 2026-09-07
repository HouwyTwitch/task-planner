using Planner.Data.Configuration;

namespace Planner.Data.Database;

public sealed class NetworkDatabaseLock
{
    private readonly PlannerSettings _settings;

    public NetworkDatabaseLock(PlannerSettings settings) => _settings = settings;

    public async Task<NetworkLockLease> AcquireAsync(CancellationToken cancellationToken = default)
    {
        var deadline = DateTime.UtcNow.AddSeconds(_settings.DatabaseLockTimeoutSeconds);
        Exception? lastError = null;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var stream = new FileStream(
                    _settings.LockFilePath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    256,
                    FileOptions.Asynchronous);

                stream.SetLength(0);
                var payload = $"{Environment.MachineName};{Environment.UserName};{Environment.ProcessId};{DateTime.UtcNow:O}";
                var bytes = System.Text.Encoding.UTF8.GetBytes(payload);
                await stream.WriteAsync(bytes, cancellationToken);
                await stream.FlushAsync(cancellationToken);
                stream.Position = 0;
                return new NetworkLockLease(stream);
            }
            catch (IOException ex)
            {
                lastError = ex;
                await Task.Delay(Random.Shared.Next(50, 151), cancellationToken);
            }
        }

        throw new TimeoutException("Сетевая база занята другим пользователем. Повторите операцию.", lastError);
    }
}

public sealed class NetworkLockLease : IAsyncDisposable, IDisposable
{
    private FileStream? _stream;
    internal NetworkLockLease(FileStream stream) => _stream = stream;
    public void Dispose() => Interlocked.Exchange(ref _stream, null)?.Dispose();
    public async ValueTask DisposeAsync()
    {
        var stream = Interlocked.Exchange(ref _stream, null);
        if (stream is not null) await stream.DisposeAsync();
    }
}
