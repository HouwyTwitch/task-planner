using Microsoft.Data.Sqlite;
using Planner.Data.Configuration;

namespace Planner.Data.Database;

public sealed class SqliteConnectionFactory
{
    private readonly PlannerSettings _settings;
    private readonly string _connectionString;

    public SqliteConnectionFactory(PlannerSettings settings)
    {
        _settings = settings;
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = settings.DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            DefaultTimeout = settings.DatabaseLockTimeoutSeconds,
            ForeignKeys = true,
            Pooling = false
        }.ToString();
    }

    public async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys=ON; PRAGMA busy_timeout=10000; PRAGMA synchronous=FULL; PRAGMA locking_mode=NORMAL;";
        await command.ExecuteNonQueryAsync(cancellationToken);
        return connection;
    }
}
