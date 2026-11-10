using Microsoft.Data.Sqlite;

namespace Planner.Data.Database;

/// <summary>
/// Единая точка доступа к общей базе.
/// <para>
/// Microsoft.Data.Sqlite не умеет настоящего асинхронного ввода-вывода: его методы *Async
/// выполняются синхронно в вызывающем потоке. База лежит в сетевой папке, поэтому каждое
/// открытие файла и каждая фиксация транзакции — это обращения к SMB-серверу. Если такой
/// код стартует в UI-потоке, окно «замирает» на всё время операции. Поэтому вся работа
/// с базой выносится в пул потоков.
/// </para>
/// </summary>
public sealed class DatabaseExecutor
{
    private readonly NetworkDatabaseLock _networkLock;
    private readonly SqliteConnectionFactory _factory;

    public DatabaseExecutor(NetworkDatabaseLock networkLock, SqliteConnectionFactory factory)
    {
        _networkLock = networkLock;
        _factory = factory;
    }

    public Task<T> WithConnectionAsync<T>(Func<SqliteConnection, Task<T>> action, CancellationToken ct = default) =>
        Task.Run(async () =>
        {
            await using var lease = await _networkLock.AcquireAsync(ct).ConfigureAwait(false);
            await using var connection = await _factory.OpenAsync(ct).ConfigureAwait(false);
            return await action(connection).ConfigureAwait(false);
        }, ct);

    public Task WithConnectionAsync(Func<SqliteConnection, Task> action, CancellationToken ct = default) =>
        WithConnectionAsync(async c => { await action(c).ConfigureAwait(false); return true; }, ct);

    public Task<T> WithTransactionAsync<T>(Func<SqliteConnection, SqliteTransaction, Task<T>> action, CancellationToken ct = default) =>
        Task.Run(async () =>
        {
            await using var lease = await _networkLock.AcquireAsync(ct).ConfigureAwait(false);
            await using var connection = await _factory.OpenAsync(ct).ConfigureAwait(false);
            using var transaction = connection.BeginTransaction();
            try
            {
                var result = await action(connection, transaction).ConfigureAwait(false);
                transaction.Commit();
                return result;
            }
            catch
            {
                try { transaction.Rollback(); } catch { }
                throw;
            }
        }, ct);
}
