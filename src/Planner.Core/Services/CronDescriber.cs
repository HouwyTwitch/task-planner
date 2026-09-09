using System.Globalization;

namespace Planner.Core.Services;

/// <summary>
/// Переводит расписание повторения в понятную русскую фразу для интерфейса.
/// Понимает оба поддерживаемых формата — Unix-cron и Quartz. Выражения, которые
/// не удалось описать словами, возвращаются без изменений, поэтому метод безопасен
/// для любых данных из базы.
/// </summary>
public static class CronDescriber
{
    private enum Gender { Masculine, Feminine, Neuter }

    private static readonly string[] WeekDaysDative =
        ["воскресеньям", "понедельникам", "вторникам", "средам", "четвергам", "пятницам", "субботам"];

    /// <summary>Винительный падеж: «в последнюю пятницу», «каждый третий понедельник».</summary>
    private static readonly string[] WeekDaysAccusative =
        ["воскресенье", "понедельник", "вторник", "среду", "четверг", "пятницу", "субботу"];

    private static readonly Gender[] WeekDayGender =
        [Gender.Neuter, Gender.Masculine, Gender.Masculine, Gender.Feminine, Gender.Masculine, Gender.Feminine, Gender.Feminine];

    private static readonly string[][] OrdinalsAccusative =
    [
        ["", "первый", "второй", "третий", "четвёртый", "пятый"],
        ["", "первую", "вторую", "третью", "четвёртую", "пятую"],
        ["", "первое", "второе", "третье", "четвёртое", "пятое"]
    ];

    private static readonly string[] EveryByGender = ["каждый", "каждую", "каждое"];
    private static readonly string[] LastByGender = ["последний", "последнюю", "последнее"];

    private static readonly string[] MonthsGenitive =
        ["января", "февраля", "марта", "апреля", "мая", "июня", "июля", "августа", "сентября", "октября", "ноября", "декабря"];

    private static readonly string[] MonthsShort =
        ["янв", "фев", "мар", "апр", "май", "июн", "июл", "авг", "сен", "окт", "ноя", "дек"];

    private static readonly string[] MonthNames =
        ["JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"];

    public static string Describe(string? cron)
    {
        if (string.IsNullOrWhiteSpace(cron)) return string.Empty;
        var raw = cron.Trim();

        NormalizedCron normalized;
        try { normalized = CronSupport.Normalize(raw); }
        catch (FormatException) { return raw; }

        var fields = normalized.Fields;
        var time = FormatTime(fields[0], fields[1], fields[2]);
        if (time is null) return raw;

        var day = fields[3];
        var month = fields[4];
        var weekday = fields[5];

        if (weekday != "*" && day == "*")
        {
            var weekdayText = DescribeWeekDay(weekday);
            if (weekdayText is null) return raw;
            return month == "*" ? $"{weekdayText} {time}" : $"{weekdayText} ({FormatMonthList(month) ?? month}) {time}";
        }

        if (weekday != "*") return raw;

        if (day == "*") return month == "*" ? $"ежедневно {time}" : $"ежедневно в месяцы {FormatMonthList(month) ?? month} {time}";
        if (day == "L") return month == "*" ? $"в последний день месяца {time}" : $"в последний день месяцев {FormatMonthList(month) ?? month} {time}";

        var dayNumbers = ParseList(day, 1, 31, null);
        if (dayNumbers is null || dayNumbers.Count != 1) return raw;
        var dayNumber = dayNumbers[0];

        if (month == "*") return $"ежемесячно {dayNumber} числа {time}";

        var months = ParseList(month, 1, 12, MonthNames);
        if (months is null || months.Count == 0) return raw;

        if (months.Count == 1) return $"ежегодно {dayNumber} {MonthsGenitive[months[0] - 1]} {time}";
        if (months.Count == 4) return $"ежеквартально, {dayNumber} числа ({FormatMonths(months)}) {time}";
        if (months.Count == 2) return $"раз в полугодие, {dayNumber} числа ({FormatMonths(months)}) {time}";
        return $"{dayNumber} числа ({FormatMonths(months)}) {time}";
    }

    /// <summary>«в 09:30» либо «в 09:30:15», если в расписании заданы ненулевые секунды.</summary>
    private static string? FormatTime(string secondText, string minuteText, string hourText)
    {
        if (!int.TryParse(secondText, NumberStyles.None, CultureInfo.InvariantCulture, out var second) ||
            !int.TryParse(minuteText, NumberStyles.None, CultureInfo.InvariantCulture, out var minute) ||
            !int.TryParse(hourText, NumberStyles.None, CultureInfo.InvariantCulture, out var hour) ||
            second is < 0 or > 59 || minute is < 0 or > 59 || hour is < 0 or > 23)
            return null;

        return second == 0 ? $"в {hour:00}:{minute:00}" : $"в {hour:00}:{minute:00}:{second:00}";
    }

    /// <summary>Описывает поле дня недели, включая формы «5L» (последняя пятница) и «5#3» (третья пятница).</summary>
    private static string? DescribeWeekDay(string weekday)
    {
        var hash = weekday.IndexOf('#');
        if (hash > 0)
        {
            var dayIndex = CronSupport.TryReadWeekDay(weekday[..hash]);
            if (dayIndex is null ||
                !int.TryParse(weekday[(hash + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out var ordinal) ||
                ordinal is < 1 or > 5)
                return null;
            var gender = (int)WeekDayGender[dayIndex.Value];
            return $"{EveryByGender[gender]} {OrdinalsAccusative[gender][ordinal]} {WeekDaysAccusative[dayIndex.Value]} месяца";
        }

        if (weekday.Length > 1 && weekday[^1] == 'L')
        {
            var dayIndex = CronSupport.TryReadWeekDay(weekday[..^1]);
            if (dayIndex is null) return null;
            var gender = (int)WeekDayGender[dayIndex.Value];
            return $"в {LastByGender[gender]} {WeekDaysAccusative[dayIndex.Value]} месяца";
        }

        var days = ParseWeekDayList(weekday);
        if (days is null || days.Count == 0) return null;

        var set = days.ToHashSet();
        if (set.SetEquals(new[] { 1, 2, 3, 4, 5 })) return "по будням";
        if (set.SetEquals(new[] { 0, 6 })) return "по выходным";
        return $"по {string.Join(", ", days.Distinct().Select(x => WeekDaysDative[x]))}";
    }

    /// <summary>
    /// Разбирает перечисление дней недели. Диапазон может переходить через воскресенье
    /// («5-1» — с пятницы по понедельник), поэтому конец диапазона может быть меньше начала.
    /// </summary>
    private static List<int>? ParseWeekDayList(string value)
    {
        var result = new List<int>();
        foreach (var token in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var dash = token.IndexOf('-');
            if (dash > 0)
            {
                var from = CronSupport.TryReadWeekDay(token[..dash]);
                var to = CronSupport.TryReadWeekDay(token[(dash + 1)..]);
                if (from is null || to is null) return null;
                var day = from.Value;
                for (var step = 0; step < 7; step++)
                {
                    result.Add(day);
                    if (day == to.Value) break;
                    day = (day + 1) % 7;
                }
                continue;
            }

            var single = CronSupport.TryReadWeekDay(token);
            if (single is null) return null;
            result.Add(single.Value);
        }
        return result.Count == 0 ? null : result;
    }

    private static string? FormatMonthList(string month)
    {
        var months = ParseList(month, 1, 12, MonthNames);
        return months is null ? null : FormatMonths(months);
    }

    private static string FormatMonths(IEnumerable<int> months) =>
        string.Join(", ", months.Select(x => MonthsShort[x - 1]));

    /// <summary>Разбирает перечисление чисел вида «1,4,7,10». Названия месяцев принимаются наравне с числами.</summary>
    private static List<int>? ParseList(string value, int min, int max, string[]? names)
    {
        var result = new List<int>();
        foreach (var token in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            int number;
            if (!int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out number))
            {
                if (names is null) return null;
                var index = Array.IndexOf(names, token.ToUpperInvariant());
                if (index < 0) return null;
                number = index + 1;
            }
            if (number < min || number > max) return null;
            result.Add(number);
        }
        return result.Count == 0 ? null : result;
    }
}
