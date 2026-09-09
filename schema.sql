-- Сетевой планировщик: SQLite schema
PRAGMA foreign_keys=ON;
PRAGMA journal_mode=DELETE;
PRAGMA synchronous=FULL;
PRAGMA busy_timeout=10000;
PRAGMA locking_mode=NORMAL;

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
    ShiftWeekendToWeekday INTEGER NOT NULL DEFAULT 0,
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
