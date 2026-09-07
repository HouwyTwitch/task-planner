using Planner.Core.Models;

namespace Planner.Core.Services;

public static class ReminderTimeConverter
{
    public static readonly IReadOnlyList<ReminderUnit> Units =
    [
        new("Секунды", 1),
        new("Минуты", 60),
        new("Часы", 3600),
        new("Дни", 86400),
        new("Недели", 604800)
    ];

    public static long ToSeconds(long value, ReminderUnit unit)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        checked { return value * unit.Multiplier; }
    }

    public static ReminderDisplay FromSeconds(long seconds)
    {
        if (seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
        foreach (var unit in Units.OrderByDescending(x => x.Multiplier))
        {
            if (seconds % unit.Multiplier == 0)
                return new ReminderDisplay(seconds / unit.Multiplier, unit);
        }
        return new ReminderDisplay(seconds, Units[0]);
    }
}
