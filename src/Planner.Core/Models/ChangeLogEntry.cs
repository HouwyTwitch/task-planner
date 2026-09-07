namespace Planner.Core.Models;

public sealed class ChangeLogEntry
{
    public long Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public long? ActorUserId { get; set; }
    public long? TargetUserId { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Message { get; set; }
}
