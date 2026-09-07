using Microsoft.Data.Sqlite;

namespace Planner.Data.Database;

public sealed class DatabaseExecutor
{
    private readonly NetworkDatabaseLock _networkLock;
    private readonly SqliteConnectionFactory _factory;

    public DatabaseExecutor(NetworkDatabaseLock networkLock, SqliteConnectionFactory factory)
    {
        _networkLock = networkLock;
        _factory = factory;
    }

    public async Task<T> WithConnectionAsync<T>(Func<SqliteConnection, Task<T>> action, CancellationToken ct = default)
    {
        await using var lease = await _networkLock.AcquireAsync(ct);
        await using var connection = await _factory.OpenAsync(ct);
        return await action(connection);
    }

    public Task WithConnectionAsync(Func<SqliteConnection, Task> action, CancellationToken ct = default) =>
        WithConnectionAsync(async c => { await action(c); return true; }, ct);

    public async Task<T> WithTransactionAsync<T>(Func<SqliteConnection, SqliteTransaction, Task<T>> action, CancellationToken ct = default)
    {
        await using var lease = await _networkLock.AcquireAsync(ct);
        await using var connection = await _factory.OpenAsync(ct);
        using var transaction = connection.BeginTransaction();
        try
        {
            var result = await action(connection, transaction);
            transaction.Commit();
            return result;
        }
        catch
        {
            try { transaction.Rollback(); } catch { }
            throw;
        }
    }
}
