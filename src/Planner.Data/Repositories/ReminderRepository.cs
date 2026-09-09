using Planner.Core.Models;
using Planner.Data.Database;

namespace Planner.Data.Repositories;

public sealed class ReminderRepository
{
    private readonly DatabaseExecutor _db;
    public ReminderRepository(DatabaseExecutor db) => _db = db;

    public Task<IReadOnlyList<DueReminder>> GetCandidateRemindersAsync(long userId, DateTime fromTrigger, DateTime toTrigger, CancellationToken ct = default) =>
        _db.WithConnectionAsync(async c =>
        {
            var list = new List<DueReminder>();
            await using var cmd = c.CreateCommand();
            cmd.CommandText = """
SELECT r.Id,t.Id,t.Title,t.StartDate,r.OffsetSeconds
FROM Reminders r JOIN Tasks t ON t.Id=r.TaskId
WHERE t.AssignedToUserId=$u AND t.StartDate IS NOT NULL
  AND t.Status <> 'Completed'
  AND NOT (t.CronSchedule IS NOT NULL AND t.SourceTaskId IS NULL)
  AND datetime(t.StartDate, '-' || r.OffsetSeconds || ' seconds') >= $from
  AND datetime(t.StartDate, '-' || r.OffsetSeconds || ' seconds') < $to;
""";
            cmd.Parameters.AddWithValue("$u", userId); cmd.Parameters.AddWithValue("$from", DbTime.ToText(fromTrigger)); cmd.Parameters.AddWithValue("$to", DbTime.ToText(toTrigger));
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct)) list.Add(new DueReminder{ReminderId=r.GetInt64(0),TaskId=r.GetInt64(1),TaskTitle=r.GetString(2),TaskStart=DbTime.Parse(r.GetString(3)),OffsetSeconds=r.GetInt64(4)});
            return (IReadOnlyList<DueReminder>)list;
        },ct);
}
