using Microsoft.Data.Sqlite;

namespace Planner.Data.Database;

/// <summary>
/// Единая точка доступа к общей сетевой базе.
/// Microsoft.Data.Sqlite не имеет настоящего асинхронного ввода-вывода: его *Async-методы
/// выполняются синхронно в вызывающем потоке. Если вызвать их из интерфейса, окно
/// «замерзает» на всё время работы с SMB (захват planner.lock, открытие файла, запрос).
/// Поэтому вся работа с базой и файлом блокировки выносится в пул потоков.
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
