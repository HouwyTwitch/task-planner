using System.Globalization;

namespace Planner.Core.Services;

/// <summary>
/// Переводит 5-польное Cron-выражение в понятную русскую фразу для интерфейса.
/// Незнакомые выражения возвращаются без изменений, поэтому метод безопасен для любых данных.
/// </summary>
public static class CronDescriber
{
    private static readonly string[] WeekDays =
        ["воскресеньям", "понедельникам", "вторникам", "средам", "четвергам", "пятницам", "субботам"];

    private static readonly string[] MonthsGenitive =
        ["января", "февраля", "марта", "апреля", "мая", "июня", "июля", "августа", "сентября", "октября", "ноября", "декабря"];

    private static readonly string[] MonthsShort =
        ["янв", "фев", "мар", "апр", "май", "июн", "июл", "авг", "сен", "окт", "ноя", "дек"];

    public static string Describe(string? cron)
    {
        if (string.IsNullOrWhiteSpace(cron)) return string.Empty;

        var parts = cron.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5) return cron.Trim();

        var minuteText = parts[0];
        var hourText = parts[1];
        var day = parts[2];
        var month = parts[3];
        var weekday = parts[4];

        if (!int.TryParse(minuteText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minute) ||
            !int.TryParse(hourText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var hour) ||
            minute is < 0 or > 59 || hour is < 0 or > 23)
            return cron.Trim();

        var time = $"в {hour:00}:{minute:00}";

        if (weekday == "1-5" && day == "*" && month == "*") return $"по будням {time}";

        if (weekday != "*" && day == "*" && month == "*")
        {
            var days = ParseList(weekday, 0, 7);
            if (days is null) return cron.Trim();
            var names = days.Select(x => WeekDays[x == 7 ? 0 : x]).Distinct().ToList();
            return $"по {string.Join(", ", names)} {time}";
        }

        if (weekday != "*") return cron.Trim();

        if (day == "*" && month == "*") return $"ежедневно {time}";

        var dayNumbers = ParseList(day, 1, 31);
        if (dayNumbers is null || dayNumbers.Count != 1) return cron.Trim();
        var dayNumber = dayNumbers[0];

        if (month == "*") return $"ежемесячно {dayNumber} числа {time}";

        var months = ParseList(month, 1, 12);
        if (months is null || months.Count == 0) return cron.Trim();

        if (months.Count == 1) return $"ежегодно {dayNumber} {MonthsGenitive[months[0] - 1]} {time}";
        if (months.Count == 4) return $"ежеквартально, {dayNumber} числа ({FormatMonths(months)}) {time}";
        if (months.Count == 2) return $"раз в полугодие, {dayNumber} числа ({FormatMonths(months)}) {time}";
        return $"{dayNumber} числа ({FormatMonths(months)}) {time}";
    }

    private static string FormatMonths(IEnumerable<int> months) =>
        string.Join(", ", months.Select(x => MonthsShort[x - 1]));

    private static List<int>? ParseList(string value, int min, int max)
    {
        var result = new List<int>();
        foreach (var token in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)) return null;
            if (number < min || number > max) return null;
            result.Add(number);
        }
        return result.Count == 0 ? null : result;
    }
}
