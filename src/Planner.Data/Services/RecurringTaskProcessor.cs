using System.Globalization;
using Cronos;
using Planner.Data.Configuration;
using Planner.Data.Database;
using Planner.Data.Repositories;

namespace Planner.Data.Services;

public sealed class RecurringTaskProcessor
{
    private readonly DatabaseExecutor _db;
    private readonly PlannerSettings _settings;
    private readonly SignalService _signals;

    public RecurringTaskProcessor(DatabaseExecutor db, PlannerSettings settings, SignalService signals)
    {
        _db=db; _settings=settings; _signals=signals;
    }

    public async Task<int> ProcessAsync(CancellationToken ct=default)
    {
        var tz=_settings.ResolveTimeZone();
        var nowLocal=TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,tz);
        var horizonLocal=nowLocal.AddMonths(Math.Max(1,_settings.RecurrenceHorizonMonths));
        var horizonUtc=TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(horizonLocal,DateTimeKind.Unspecified),tz);
        var targets=new HashSet<long>();

        var created=await _db.WithTransactionAsync(async (c,tx)=>
        {
            var templates=new List<TemplateRow>();
            await using(var cmd=c.CreateCommand())
            {
                cmd.Transaction=tx;
                cmd.CommandText="""
SELECT t.Id,t.Title,t.Description,t.CreatedByUserId,t.AssignedToUserId,t.StartDate,t.DurationSeconds,t.IsAllDay,t.CronSchedule,t.CreatedAt,
       (SELECT MAX(re.OccurrenceUtc) FROM RecurrenceExecutions re WHERE re.TaskId=t.Id) AS LastOccurrenceUtc
FROM Tasks t
WHERE t.CronSchedule IS NOT NULL AND t.SourceTaskId IS NULL AND t.Status <> 'Completed';
""";
                await using var r=await cmd.ExecuteReaderAsync(ct);
                while(await r.ReadAsync(ct))
                {
                    templates.Add(new TemplateRow(
                        r.GetInt64(0),r.GetString(1),r.IsDBNull(2)?null:r.GetString(2),r.GetInt64(3),r.GetInt64(4),
                        r.IsDBNull(5)?null:DbTime.Parse(r.GetString(5)),r.IsDBNull(6)?null:r.GetInt64(6),r.GetInt64(7)!=0,r.GetString(8),
                        DbTime.Parse(r.GetString(9)),r.IsDBNull(10)?null:DateTime.Parse(r.GetString(10),CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind)));
                }
            }

            var count=0;
            foreach(var t in templates)
            {
                CronExpression expression;
                try { expression=CronExpression.Parse(t.Cron); }
                catch(CronFormatException) { continue; }

                var createdUtc=TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(t.CreatedAt,DateTimeKind.Unspecified),tz);
                var firstAllowedUtc=t.StartDate is null
                    ? createdUtc
                    : TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(t.StartDate.Value,DateTimeKind.Unspecified),tz);
                var fromUtc=t.LastOccurrenceUtc?.AddSeconds(1) ?? firstAllowedUtc;
                if(fromUtc<firstAllowedUtc) fromUtc=firstAllowedUtc;

                var templateStartLocal=t.StartDate ?? t.CreatedAt;
                var templateHorizonLocal=templateStartLocal.AddMonths(Math.Max(1,_settings.RecurrenceHorizonMonths));
                var templateHorizonUtc=TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(templateHorizonLocal,DateTimeKind.Unspecified),tz);
                var untilUtc=templateHorizonUtc>horizonUtc?templateHorizonUtc:horizonUtc;
                if(fromUtc>untilUtc) continue;

                var occurrences=expression.GetOccurrences(fromUtc,untilUtc,tz,true,true).Take(5000).ToList();
                foreach(var occurrenceUtc in occurrences)
                {
                    var local=TimeZoneInfo.ConvertTimeFromUtc(occurrenceUtc,tz);
                    await using var guard=c.CreateCommand(); guard.Transaction=tx;
                    guard.CommandText="INSERT OR IGNORE INTO RecurrenceExecutions(TaskId,OccurrenceUtc,CreatedAt) VALUES($id,$occ,$now)";
                    guard.Parameters.AddWithValue("$id",t.Id); guard.Parameters.AddWithValue("$occ",occurrenceUtc.ToString("O",CultureInfo.InvariantCulture)); guard.Parameters.AddWithValue("$now",DbTime.ToText(DateTime.Now));
                    if(await guard.ExecuteNonQueryAsync(ct)!=1) continue;

                    var duration=t.AllDay?0:(t.Duration is >0?t.Duration.Value:3600);
                    var start=t.AllDay?local.Date:local;
                    var end=t.AllDay?start.Date:start.AddSeconds(duration);
                    await using var ins=c.CreateCommand(); ins.Transaction=tx;
                    ins.CommandText="""
INSERT INTO Tasks(Title,Description,CreatedByUserId,AssignedToUserId,StartDate,EndDate,DurationSeconds,IsAllDay,CronSchedule,Status,SourceTaskId,CreatedAt,UpdatedAt)
VALUES($title,$desc,$cb,$a,$s,$e,$dur,$all,NULL,'Pending',$src,$now,$now); SELECT last_insert_rowid();
""";
                    ins.Parameters.AddWithValue("$title",t.Title); ins.Parameters.AddWithValue("$desc",(object?)t.Description??DBNull.Value); ins.Parameters.AddWithValue("$cb",t.CreatedBy); ins.Parameters.AddWithValue("$a",t.Assigned);
                    ins.Parameters.AddWithValue("$s",DbTime.ToText(start)); ins.Parameters.AddWithValue("$e",DbTime.ToText(end)); ins.Parameters.AddWithValue("$dur",duration); ins.Parameters.AddWithValue("$all",t.AllDay?1:0);
                    ins.Parameters.AddWithValue("$src",t.Id); ins.Parameters.AddWithValue("$now",DbTime.ToText(DateTime.Now));
                    var newId=Convert.ToInt64(await ins.ExecuteScalarAsync(ct));

                    await using var copyRem=c.CreateCommand(); copyRem.Transaction=tx;
                    copyRem.CommandText="INSERT INTO Reminders(TaskId,OffsetSeconds,IsTriggered) SELECT $newId,OffsetSeconds,0 FROM Reminders WHERE TaskId=$src";
                    copyRem.Parameters.AddWithValue("$newId",newId); copyRem.Parameters.AddWithValue("$src",t.Id); await copyRem.ExecuteNonQueryAsync(ct);
                    await TaskRepository.InsertChangeAsync(c,tx,"Task",newId,"RecurringCreated",t.CreatedBy,t.Assigned,$"Создан повтор задачи «{t.Title}»",ct);
                    targets.Add(t.Assigned); count++;
                }
            }
            return count;
        },ct);

        foreach(var target in targets) try { await _signals.SignalUserAsync(target,ct); } catch { }
        return created;
    }

    private sealed record TemplateRow(long Id,string Title,string? Description,long CreatedBy,long Assigned,DateTime? StartDate,long? Duration,bool AllDay,string Cron,DateTime CreatedAt,DateTime? LastOccurrenceUtc);
}
