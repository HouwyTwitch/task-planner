using Microsoft.Data.Sqlite;
using Planner.Core.Models;
using Planner.Data.Database;

namespace Planner.Data.Repositories;

public sealed class ChangeLogRepository
{
    private readonly DatabaseExecutor _db;
    public ChangeLogRepository(DatabaseExecutor db) => _db = db;

    public Task<long> GetMaxIdAsync(CancellationToken ct = default) => _db.WithConnectionAsync(async c =>
    {
        await using var cmd=c.CreateCommand(); cmd.CommandText="SELECT COALESCE(MAX(Id),0) FROM ChangeLog";
        return Convert.ToInt64(await cmd.ExecuteScalarAsync(ct));
    }, ct);

    public Task<(long MaxId, IReadOnlyList<ChangeLogEntry> Entries)> GetBatchForUserAsync(long afterId, long userId, CancellationToken ct = default) => _db.WithConnectionAsync(async c =>
    {
        await using var maxCmd=c.CreateCommand();
        maxCmd.CommandText="SELECT COALESCE(MAX(Id),0) FROM ChangeLog";
        var maxId=Convert.ToInt64(await maxCmd.ExecuteScalarAsync(ct));

        var list=new List<ChangeLogEntry>();
        await using var cmd=c.CreateCommand();
        cmd.CommandText="SELECT Id,EntityType,EntityId,ChangeType,ActorUserId,TargetUserId,ChangedAt,Message FROM ChangeLog WHERE Id>$id AND Id<=$max AND (TargetUserId=$u OR TargetUserId IS NULL) ORDER BY Id";
        cmd.Parameters.AddWithValue("$id",afterId); cmd.Parameters.AddWithValue("$max",maxId); cmd.Parameters.AddWithValue("$u",userId);
        await using var r=await cmd.ExecuteReaderAsync(ct);
        while(await r.ReadAsync(ct)) list.Add(new ChangeLogEntry{Id=r.GetInt64(0),EntityType=r.GetString(1),EntityId=r.GetInt64(2),ChangeType=r.GetString(3),ActorUserId=r.IsDBNull(4)?null:r.GetInt64(4),TargetUserId=r.IsDBNull(5)?null:r.GetInt64(5),ChangedAt=DbTime.Parse(r.GetString(6)),Message=r.IsDBNull(7)?null:r.GetString(7)});
        return (maxId, (IReadOnlyList<ChangeLogEntry>)list);
    },ct);

}
