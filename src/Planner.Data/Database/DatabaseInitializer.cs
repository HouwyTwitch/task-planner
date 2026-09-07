using Planner.Data.Configuration;

namespace Planner.Data.Database;

public sealed class DatabaseInitializer
{
    private readonly PlannerSettings _settings;
    private readonly DatabaseExecutor _db;

    public DatabaseInitializer(PlannerSettings settings, DatabaseExecutor db)
    {
        _settings = settings;
        _db = db;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        Directory.CreateDirectory(_settings.SharedFolder);
        Directory.CreateDirectory(_settings.SignalsFolder);

        await _db.WithConnectionAsync(async connection =>
        {
            await using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA journal_mode=DELETE;";
                await pragma.ExecuteScalarAsync(ct);
            }

            await using (var command = connection.CreateCommand())
            {
                command.CommandText = SchemaSql;
                await command.ExecuteNonQueryAsync(ct);
            }

            // Migration path from the original specification schema.
            await EnsureColumnAsync(connection, "Tasks", "SourceTaskId", "INTEGER NULL", ct);
            await EnsureColumnAsync(connection, "Tasks", "CreatedAt", "TEXT NULL", ct);
            await EnsureColumnAsync(connection, "Tasks", "UpdatedAt", "TEXT NULL", ct);
            await using (var backfill = connection.CreateCommand())
            {
                backfill.CommandText = "UPDATE Tasks SET CreatedAt=COALESCE(CreatedAt, datetime('now','localtime')), UpdatedAt=COALESCE(UpdatedAt, CreatedAt, datetime('now','localtime')); UPDATE Tasks SET StartDate=COALESCE(StartDate, date('now','localtime')), IsAllDay=CASE WHEN StartDate IS NULL THEN 1 ELSE IsAllDay END; UPDATE Tasks SET EndDate=COALESCE(EndDate, StartDate) WHERE IsAllDay=1; PRAGMA user_version=3;";
                await backfill.ExecuteNonQueryAsync(ct);
            }
        }, ct);
    }

    private static async Task EnsureColumnAsync(Microsoft.Data.Sqlite.SqliteConnection connection, string table, string column, string definition, CancellationToken ct)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var info = connection.CreateCommand())
        {
            info.CommandText = $"PRAGMA table_info({table})";
            await using var reader = await info.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) columns.Add(reader.GetString(1));
        }
        if (columns.Contains(column)) return;
        await using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition}";
        await alter.ExecuteNonQueryAsync(ct);
    }

    private const string SchemaSql = """
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    DisplayName TEXT NOT NULL,
    ParentId INTEGER NULL,
    Role TEXT NOT NULL DEFAULT 'User',
    FOREIGN KEY(ParentId) REFERENCES Users(Id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS Tasks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT,
    CreatedByUserId INTEGER NOT NULL,
    AssignedToUserId INTEGER NOT NULL,
    StartDate TEXT NOT NULL,
    EndDate TEXT NULL,
    DurationSeconds INTEGER NULL,
    IsAllDay INTEGER NOT NULL DEFAULT 0,
    CronSchedule TEXT NULL,
    Status TEXT NOT NULL DEFAULT 'Pending',
    SourceTaskId INTEGER NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    FOREIGN KEY(CreatedByUserId) REFERENCES Users(Id),
    FOREIGN KEY(AssignedToUserId) REFERENCES Users(Id),
    FOREIGN KEY(SourceTaskId) REFERENCES Tasks(Id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS Reminders (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TaskId INTEGER NOT NULL,
    OffsetSeconds INTEGER NOT NULL,
    IsTriggered INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY(TaskId) REFERENCES Tasks(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ChangeLog (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityType TEXT NOT NULL,
    EntityId INTEGER NOT NULL,
    ChangeType TEXT NOT NULL,
    ActorUserId INTEGER NULL,
    TargetUserId INTEGER NULL,
    ChangedAt TEXT NOT NULL,
    Message TEXT NULL
);

CREATE TABLE IF NOT EXISTS RecurrenceExecutions (
    TaskId INTEGER NOT NULL,
    OccurrenceUtc TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    PRIMARY KEY(TaskId, OccurrenceUtc),
    FOREIGN KEY(TaskId) REFERENCES Tasks(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_Users_ParentId ON Users(ParentId);
CREATE INDEX IF NOT EXISTS IX_Tasks_Assigned_StartDate ON Tasks(AssignedToUserId, StartDate);
CREATE INDEX IF NOT EXISTS IX_Tasks_CreatedBy ON Tasks(CreatedByUserId);
CREATE INDEX IF NOT EXISTS IX_Tasks_Cron ON Tasks(CronSchedule) WHERE CronSchedule IS NOT NULL;
CREATE INDEX IF NOT EXISTS IX_Reminders_TaskId ON Reminders(TaskId);
CREATE INDEX IF NOT EXISTS IX_ChangeLog_Target_Id ON ChangeLog(TargetUserId, Id);
""";
}
