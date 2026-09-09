using System.Globalization;
using Microsoft.Data.Sqlite;
using Planner.Core.Models;
using Planner.Data.Database;

namespace Planner.Data.Repositories;

public sealed class TaskRepository
{
    private readonly DatabaseExecutor _db;
    public TaskRepository(DatabaseExecutor db) => _db = db;

    public Task<IReadOnlyList<TaskItem>> GetForUserAsync(long userId, DateTime from, DateTime to, bool includeCreatedBy = false, CancellationToken ct = default) =>
        _db.WithConnectionAsync(async c =>
        {
            var result = new List<TaskItem>();
            await using var cmd = c.CreateCommand();
            cmd.CommandText = """
SELECT t.Id,t.Title,t.Description,t.CreatedByUserId,t.AssignedToUserId,t.StartDate,t.EndDate,t.DurationSeconds,t.IsAllDay,t.CronSchedule,t.Status,t.SourceTaskId,t.CreatedAt,t.UpdatedAt,
       COALESCE(a.DisplayName,''),COALESCE(cb.DisplayName,'')
FROM Tasks t
LEFT JOIN Users a ON a.Id=t.AssignedToUserId
LEFT JOIN Users cb ON cb.Id=t.CreatedByUserId
WHERE (($includeCreated=1 AND (t.AssignedToUserId=$uid OR t.CreatedByUserId=$uid)) OR ($includeCreated=0 AND t.AssignedToUserId=$uid))
  AND t.StartDate IS NOT NULL AND t.StartDate >= $from AND t.StartDate < $to
  AND NOT (t.CronSchedule IS NOT NULL AND t.SourceTaskId IS NULL)
ORDER BY t.StartDate, t.IsAllDay DESC, t.Id;
""";
            cmd.Parameters.AddWithValue("$uid", userId);
            cmd.Parameters.AddWithValue("$includeCreated", includeCreatedBy ? 1 : 0);
            cmd.Parameters.AddWithValue("$from", DbTime.ToText(from));
            cmd.Parameters.AddWithValue("$to", DbTime.ToText(to));
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) result.Add(ReadTask(r));
            return (IReadOnlyList<TaskItem>)result;
        }, ct);

    public Task<IReadOnlyList<TaskItem>> GetRecurringTemplatesAsync(long userId, bool includeCreatedBy = false, CancellationToken ct = default) =>
        _db.WithConnectionAsync(async c =>
        {
            var result = new List<TaskItem>();
            await using var cmd = c.CreateCommand();
            cmd.CommandText = """
SELECT t.Id,t.Title,t.Description,t.CreatedByUserId,t.AssignedToUserId,t.StartDate,t.EndDate,t.DurationSeconds,t.IsAllDay,t.CronSchedule,t.Status,t.SourceTaskId,t.CreatedAt,t.UpdatedAt,
       COALESCE(a.DisplayName,''),COALESCE(cb.DisplayName,'')
FROM Tasks t
LEFT JOIN Users a ON a.Id=t.AssignedToUserId
LEFT JOIN Users cb ON cb.Id=t.CreatedByUserId
WHERE (($includeCreated=1 AND (t.AssignedToUserId=$uid OR t.CreatedByUserId=$uid)) OR ($includeCreated=0 AND t.AssignedToUserId=$uid))
  AND t.CronSchedule IS NOT NULL AND t.SourceTaskId IS NULL
ORDER BY t.Id DESC;
""";
            cmd.Parameters.AddWithValue("$uid", userId);
            cmd.Parameters.AddWithValue("$includeCreated", includeCreatedBy ? 1 : 0);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) result.Add(ReadTask(r));
            return (IReadOnlyList<TaskItem>)result;
        }, ct);

    /// <summary>
    /// Задачи нескольких сотрудников за период — основа отчёта по подчинённым.
    /// Шаблоны повторения исключены: считаются только реальные экземпляры задач.
    /// </summary>
    public Task<IReadOnlyList<TaskItem>> GetForUsersAsync(IReadOnlyList<long> userIds, DateTime from, DateTime to, CancellationToken ct = default) =>
        _db.WithConnectionAsync(async c =>
        {
            var result = new List<TaskItem>();
            if (userIds.Count == 0) return (IReadOnlyList<TaskItem>)result;

            await using var cmd = c.CreateCommand();
            var placeholders = new string[userIds.Count];
            for (var i = 0; i < userIds.Count; i++)
            {
                placeholders[i] = "$u" + i.ToString(CultureInfo.InvariantCulture);
                cmd.Parameters.AddWithValue(placeholders[i], userIds[i]);
            }

            cmd.CommandText = $"""
SELECT t.Id,t.Title,t.Description,t.CreatedByUserId,t.AssignedToUserId,t.StartDate,t.EndDate,t.DurationSeconds,t.IsAllDay,t.CronSchedule,t.Status,t.SourceTaskId,t.CreatedAt,t.UpdatedAt,
       COALESCE(a.DisplayName,''),COALESCE(cb.DisplayName,'')
FROM Tasks t
LEFT JOIN Users a ON a.Id=t.AssignedToUserId
LEFT JOIN Users cb ON cb.Id=t.CreatedByUserId
WHERE t.AssignedToUserId IN ({string.Join(",", placeholders)})
  AND t.StartDate IS NOT NULL AND t.StartDate >= $from AND t.StartDate < $to
  AND NOT (t.CronSchedule IS NOT NULL AND t.SourceTaskId IS NULL)
ORDER BY t.StartDate, t.Id;
""";
            cmd.Parameters.AddWithValue("$from", DbTime.ToText(from));
            cmd.Parameters.AddWithValue("$to", DbTime.ToText(to));
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) result.Add(ReadTask(r));
            return (IReadOnlyList<TaskItem>)result;
        }, ct);

    public Task<TaskItem?> GetByIdAsync(long id, CancellationToken ct = default) =>
        _db.WithConnectionAsync(async c =>
        {
            await using var cmd = c.CreateCommand();
            cmd.CommandText = """
SELECT t.Id,t.Title,t.Description,t.CreatedByUserId,t.AssignedToUserId,t.StartDate,t.EndDate,t.DurationSeconds,t.IsAllDay,t.CronSchedule,t.Status,t.SourceTaskId,t.CreatedAt,t.UpdatedAt,
       COALESCE(a.DisplayName,''),COALESCE(cb.DisplayName,'')
FROM Tasks t
LEFT JOIN Users a ON a.Id=t.AssignedToUserId
LEFT JOIN Users cb ON cb.Id=t.CreatedByUserId
WHERE t.Id=$id;
""";
            cmd.Parameters.AddWithValue("$id", id);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            return await r.ReadAsync(ct) ? ReadTask(r) : null;
        }, ct);

    public Task<long> CreateAsync(TaskItem task, long? reminderOffsetSeconds, long actorUserId, bool actorIsAdmin, string? message, CancellationToken ct = default) =>
        _db.WithTransactionAsync(async (c, tx) =>
        {
            await EnsureCanCreateForAsync(c, tx, actorUserId, actorIsAdmin, task.AssignedToUserId, ct);
            var now = DateTime.Now;
            task.CreatedAt = now; task.UpdatedAt = now;
            await using var cmd = c.CreateCommand(); cmd.Transaction = tx;
            cmd.CommandText = """
INSERT INTO Tasks(Title,Description,CreatedByUserId,AssignedToUserId,StartDate,EndDate,DurationSeconds,IsAllDay,CronSchedule,Status,SourceTaskId,CreatedAt,UpdatedAt)
VALUES($t,$d,$cb,$a,$s,$e,$dur,$all,$cron,$status,$src,$ca,$ua);
SELECT last_insert_rowid();
""";
            AddTaskParameters(cmd, task);
            var id = Convert.ToInt64(await cmd.ExecuteScalarAsync(ct)); task.Id = id;
            await ReplaceReminderAsync(c,tx,id,reminderOffsetSeconds,ct);
            await InsertChangeAsync(c, tx, "Task", id, "Created", actorUserId, task.AssignedToUserId, message, ct);
            return id;
        }, ct);

    public Task UpdateManagedAsync(TaskItem task, long? reminderOffsetSeconds, long actorUserId, bool actorIsAdmin, string? message, CancellationToken ct = default) =>
        _db.WithTransactionAsync(async (c, tx) =>
        {
            await EnsureCanManageTaskAsync(c, tx, actorUserId, actorIsAdmin, task.Id, ct);
            await EnsureCanCreateForAsync(c, tx, actorUserId, actorIsAdmin, task.AssignedToUserId, ct);
            task.UpdatedAt = DateTime.Now;
            await using var cmd = c.CreateCommand(); cmd.Transaction = tx;
            cmd.CommandText = """
UPDATE Tasks SET Title=$t,Description=$d,AssignedToUserId=$a,StartDate=$s,EndDate=$e,DurationSeconds=$dur,
IsAllDay=$all,CronSchedule=$cron,Status=$status,UpdatedAt=$ua WHERE Id=$id;
""";
            AddTaskParameters(cmd, task); cmd.Parameters.AddWithValue("$id", task.Id);
            await cmd.ExecuteNonQueryAsync(ct);
            await ReplaceReminderAsync(c,tx,task.Id,reminderOffsetSeconds,ct);
            await InsertChangeAsync(c, tx, "Task", task.Id, "Updated", actorUserId, task.AssignedToUserId, message, ct);
            return true;
        }, ct);

    public Task UpdateProgressAsync(long taskId,string? description,string status,long actorUserId,bool actorIsAdmin,long targetUserId,CancellationToken ct=default) =>
        _db.WithTransactionAsync(async (c,tx) =>
        {
            await EnsureCanUpdateProgressAsync(c,tx,actorUserId,actorIsAdmin,taskId,ct);
            await using var cmd=c.CreateCommand(); cmd.Transaction=tx;
            cmd.CommandText="UPDATE Tasks SET Description=$d,Status=$s,UpdatedAt=$u WHERE Id=$id";
            cmd.Parameters.AddWithValue("$d",(object?)description??DBNull.Value); cmd.Parameters.AddWithValue("$s",status);
            cmd.Parameters.AddWithValue("$u",DbTime.ToText(DateTime.Now)); cmd.Parameters.AddWithValue("$id",taskId);
            await cmd.ExecuteNonQueryAsync(ct);
            await InsertChangeAsync(c,tx,"Task",taskId,"Progress",actorUserId,targetUserId,"Обновлены статус или ход выполнения задачи",ct);
            return true;
        },ct);

    public Task UpdateStatusAsync(long taskId,string status,long actorUserId,bool actorIsAdmin,long targetUserId,CancellationToken ct=default) =>
        _db.WithTransactionAsync(async (c,tx) =>
        {
            await EnsureCanUpdateProgressAsync(c,tx,actorUserId,actorIsAdmin,taskId,ct);
            await using var cmd=c.CreateCommand(); cmd.Transaction=tx;
            cmd.CommandText="UPDATE Tasks SET Status=$s,UpdatedAt=$u WHERE Id=$id";
            cmd.Parameters.AddWithValue("$s",status); cmd.Parameters.AddWithValue("$u",DbTime.ToText(DateTime.Now)); cmd.Parameters.AddWithValue("$id",taskId);
            await cmd.ExecuteNonQueryAsync(ct);
            await InsertChangeAsync(c,tx,"Task",taskId,"Status",actorUserId,targetUserId,$"Статус задачи: {TaskStatuses.ToRussian(status)}",ct);
            return true;
        },ct);

    public Task UpdateScheduleAsync(long taskId, DateTime start, DateTime end, long actorUserId, bool actorIsAdmin, long targetUserId, CancellationToken ct = default) =>
        _db.WithTransactionAsync(async (c, tx) =>
        {
            await EnsureCanManageTaskAsync(c, tx, actorUserId, actorIsAdmin, taskId, ct);
            await using var cmd = c.CreateCommand(); cmd.Transaction = tx;
            cmd.CommandText = "UPDATE Tasks SET StartDate=$s,EndDate=$e,DurationSeconds=$dur,IsAllDay=0,UpdatedAt=$u WHERE Id=$id";
            cmd.Parameters.AddWithValue("$s", DbTime.ToText(start)); cmd.Parameters.AddWithValue("$e", DbTime.ToText(end));
            cmd.Parameters.AddWithValue("$dur", (long)(end-start).TotalSeconds); cmd.Parameters.AddWithValue("$u", DbTime.ToText(DateTime.Now)); cmd.Parameters.AddWithValue("$id", taskId);
            await cmd.ExecuteNonQueryAsync(ct);
            await InsertChangeAsync(c, tx, "Task", taskId, "Rescheduled", actorUserId, targetUserId, "Время задачи изменено", ct);
            return true;
        }, ct);

    public Task TransferAsync(long taskId,long newAssigneeId,long actorUserId,bool actorIsAdmin,CancellationToken ct=default) =>
        _db.WithTransactionAsync(async (c,tx) =>
        {
            await EnsureCanManageTaskAsync(c,tx,actorUserId,actorIsAdmin,taskId,ct);
            await EnsureCanCreateForAsync(c,tx,actorUserId,actorIsAdmin,newAssigneeId,ct);
            await using var cmd=c.CreateCommand();cmd.Transaction=tx;
            cmd.CommandText="UPDATE Tasks SET AssignedToUserId=$a,UpdatedAt=$u WHERE Id=$id";
            cmd.Parameters.AddWithValue("$a",newAssigneeId);cmd.Parameters.AddWithValue("$u",DbTime.ToText(DateTime.Now));cmd.Parameters.AddWithValue("$id",taskId);
            await cmd.ExecuteNonQueryAsync(ct);
            await InsertChangeAsync(c,tx,"Task",taskId,"Transferred",actorUserId,newAssigneeId,"Вам передана задача",ct);
            return true;
        },ct);

    public Task DeleteAsync(long taskId,long actorUserId,bool actorIsAdmin,long targetUserId,CancellationToken ct=default) =>
        _db.WithTransactionAsync(async (c,tx) =>
        {
            await EnsureCanManageTaskAsync(c,tx,actorUserId,actorIsAdmin,taskId,ct);
            await using(var children=c.CreateCommand())
            {
                children.Transaction=tx;children.CommandText="DELETE FROM Tasks WHERE SourceTaskId=$id";children.Parameters.AddWithValue("$id",taskId);await children.ExecuteNonQueryAsync(ct);
            }
            await using var cmd=c.CreateCommand();cmd.Transaction=tx;cmd.CommandText="DELETE FROM Tasks WHERE Id=$id";cmd.Parameters.AddWithValue("$id",taskId);
            await cmd.ExecuteNonQueryAsync(ct);
            await InsertChangeAsync(c,tx,"Task",taskId,"Deleted",actorUserId,targetUserId,"Задача удалена",ct);
            return true;
        },ct);

    public Task DeleteFutureGeneratedAsync(long sourceTaskId,DateTime from,CancellationToken ct=default) =>
        _db.WithTransactionAsync(async (c,tx) =>
        {
            await using(var del=c.CreateCommand())
            {
                del.Transaction=tx;del.CommandText="DELETE FROM Tasks WHERE SourceTaskId=$src AND StartDate >= $from";
                del.Parameters.AddWithValue("$src",sourceTaskId);del.Parameters.AddWithValue("$from",DbTime.ToText(from));await del.ExecuteNonQueryAsync(ct);
            }
            await using(var delGuard=c.CreateCommand())
            {
                delGuard.Transaction=tx;delGuard.CommandText="DELETE FROM RecurrenceExecutions WHERE TaskId=$src AND OccurrenceUtc >= $fromUtc";
                delGuard.Parameters.AddWithValue("$src",sourceTaskId);delGuard.Parameters.AddWithValue("$fromUtc",from.ToUniversalTime().ToString("O"));await delGuard.ExecuteNonQueryAsync(ct);
            }
            return true;
        },ct);

    public Task<long?> GetReminderOffsetAsync(long taskId, CancellationToken ct = default) =>
        _db.WithConnectionAsync<long?>(async c =>
        {
            await using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT OffsetSeconds FROM Reminders WHERE TaskId=$id ORDER BY Id LIMIT 1";
            cmd.Parameters.AddWithValue("$id", taskId);
            var value = await cmd.ExecuteScalarAsync(ct);
            return value is null or DBNull ? (long?)null : Convert.ToInt64(value);
        }, ct);

    private static async Task ReplaceReminderAsync(SqliteConnection c,SqliteTransaction tx,long taskId,long? offset,CancellationToken ct)
    {
        await using var del=c.CreateCommand();del.Transaction=tx;del.CommandText="DELETE FROM Reminders WHERE TaskId=$id";del.Parameters.AddWithValue("$id",taskId);await del.ExecuteNonQueryAsync(ct);
        if(offset is null)return;
        await using var rem=c.CreateCommand();rem.Transaction=tx;rem.CommandText="INSERT INTO Reminders(TaskId,OffsetSeconds,IsTriggered) VALUES($id,$off,0)";
        rem.Parameters.AddWithValue("$id",taskId);rem.Parameters.AddWithValue("$off",offset.Value);await rem.ExecuteNonQueryAsync(ct);
    }

    private static async Task EnsureCanCreateForAsync(SqliteConnection c, SqliteTransaction tx, long actorUserId, bool actorIsAdmin, long targetUserId, CancellationToken ct)
    {
        if (actorIsAdmin || actorUserId == targetUserId) return;
        await using var cmd = c.CreateCommand(); cmd.Transaction = tx;
        cmd.CommandText = """
WITH RECURSIVE Subordinates AS (
    SELECT Id, ParentId FROM Users WHERE ParentId=$actor
    UNION ALL
    SELECT u.Id, u.ParentId FROM Users u JOIN Subordinates s ON u.ParentId=s.Id
)
SELECT EXISTS(SELECT 1 FROM Subordinates WHERE Id=$target);
""";
        cmd.Parameters.AddWithValue("$actor", actorUserId); cmd.Parameters.AddWithValue("$target", targetUserId);
        if (Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)) != 1) throw new UnauthorizedAccessException("Нет права назначать задачу выбранному пользователю.");
    }

    private static async Task EnsureCanManageTaskAsync(SqliteConnection c, SqliteTransaction tx, long actorUserId, bool actorIsAdmin, long taskId, CancellationToken ct)
    {
        if (actorIsAdmin) return;
        await using var cmd=c.CreateCommand();cmd.Transaction=tx;cmd.CommandText="SELECT CreatedByUserId FROM Tasks WHERE Id=$id";cmd.Parameters.AddWithValue("$id",taskId);
        var value=await cmd.ExecuteScalarAsync(ct);
        if(value is null or DBNull)throw new KeyNotFoundException("Задача не найдена.");
        if(Convert.ToInt64(value)!=actorUserId)throw new UnauthorizedAccessException("Полностью редактировать, переносить, передавать и удалять задачу может только её создатель или администратор.");
    }

    private static async Task EnsureCanUpdateProgressAsync(SqliteConnection c,SqliteTransaction tx,long actorUserId,bool actorIsAdmin,long taskId,CancellationToken ct)
    {
        if(actorIsAdmin)return;
        await using var cmd=c.CreateCommand();cmd.Transaction=tx;cmd.CommandText="SELECT CreatedByUserId,AssignedToUserId FROM Tasks WHERE Id=$id";cmd.Parameters.AddWithValue("$id",taskId);
        await using var r=await cmd.ExecuteReaderAsync(ct);
        if(!await r.ReadAsync(ct))throw new KeyNotFoundException("Задача не найдена.");
        if(r.GetInt64(0)!=actorUserId && r.GetInt64(1)!=actorUserId)throw new UnauthorizedAccessException("Изменять статус и описание может создатель или исполнитель задачи.");
    }

    private static void AddTaskParameters(SqliteCommand cmd, TaskItem t)
    {
        cmd.Parameters.AddWithValue("$t", t.Title.Trim()); cmd.Parameters.AddWithValue("$d", (object?)t.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$cb", t.CreatedByUserId); cmd.Parameters.AddWithValue("$a", t.AssignedToUserId);
        cmd.Parameters.AddWithValue("$s", (object?)DbTime.ToText(t.StartDate) ?? DBNull.Value); cmd.Parameters.AddWithValue("$e", (object?)DbTime.ToText(t.EndDate) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$dur", (object?)t.DurationSeconds ?? DBNull.Value); cmd.Parameters.AddWithValue("$all", t.IsAllDay ? 1 : 0);
        cmd.Parameters.AddWithValue("$cron", (object?)t.CronSchedule ?? DBNull.Value); cmd.Parameters.AddWithValue("$status", t.Status);
        cmd.Parameters.AddWithValue("$src", (object?)t.SourceTaskId ?? DBNull.Value); cmd.Parameters.AddWithValue("$ca", DbTime.ToText(t.CreatedAt == default ? DateTime.Now : t.CreatedAt));
        cmd.Parameters.AddWithValue("$ua", DbTime.ToText(t.UpdatedAt == default ? DateTime.Now : t.UpdatedAt));
    }

    internal static async Task InsertChangeAsync(SqliteConnection c, SqliteTransaction tx, string entityType, long entityId, string changeType, long? actorId, long? targetId, string? message, CancellationToken ct)
    {
        await using var log = c.CreateCommand(); log.Transaction = tx;
        log.CommandText = "INSERT INTO ChangeLog(EntityType,EntityId,ChangeType,ActorUserId,TargetUserId,ChangedAt,Message) VALUES($et,$eid,$ct,$a,$t,$dt,$m)";
        log.Parameters.AddWithValue("$et", entityType); log.Parameters.AddWithValue("$eid", entityId); log.Parameters.AddWithValue("$ct", changeType);
        log.Parameters.AddWithValue("$a", (object?)actorId ?? DBNull.Value); log.Parameters.AddWithValue("$t", (object?)targetId ?? DBNull.Value);
        log.Parameters.AddWithValue("$dt", DbTime.ToText(DateTime.Now)); log.Parameters.AddWithValue("$m", (object?)message ?? DBNull.Value);
        await log.ExecuteNonQueryAsync(ct);
    }

    internal static TaskItem ReadTask(SqliteDataReader r) => new()
    {
        Id=r.GetInt64(0), Title=r.GetString(1), Description=r.IsDBNull(2)?null:r.GetString(2), CreatedByUserId=r.GetInt64(3), AssignedToUserId=r.GetInt64(4),
        StartDate=DbTime.ParseNullable(r.GetValue(5)), EndDate=DbTime.ParseNullable(r.GetValue(6)), DurationSeconds=r.IsDBNull(7)?null:r.GetInt64(7), IsAllDay=r.GetInt64(8)!=0,
        CronSchedule=r.IsDBNull(9)?null:r.GetString(9), Status=r.GetString(10), SourceTaskId=r.IsDBNull(11)?null:r.GetInt64(11), CreatedAt=DbTime.Parse(r.GetString(12)), UpdatedAt=DbTime.Parse(r.GetString(13)),
        AssignedToDisplayName=r.FieldCount>14 && !r.IsDBNull(14)?r.GetString(14):string.Empty, CreatedByDisplayName=r.FieldCount>15 && !r.IsDBNull(15)?r.GetString(15):string.Empty
    };
}
