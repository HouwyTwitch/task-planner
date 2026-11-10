using System.Globalization;
using Cronos;

namespace Planner.Core.Services;

/// <summary>Разобранное расписание, приведённое к виду, понятному библиотеке Cronos.</summary>
/// <param name="Expression">Выражение для Cronos: пять полей либо шесть, если заданы секунды.</param>
/// <param name="IncludeSeconds">Первое поле выражения — секунды.</param>
/// <param name="IsQuartz">Исходное выражение было записано в формате Quartz.</param>
/// <param name="Fields">Всегда шесть полей: секунды, минуты, часы, день месяца, месяц, день недели.</param>
public sealed record NormalizedCron(string Expression, bool IncludeSeconds, bool IsQuartz, IReadOnlyList<string> Fields);

/// <summary>
/// Единая точка разбора расписаний повторения. Поддерживаются два формата:
/// <list type="bullet">
/// <item>классический Unix-cron из пяти полей: минуты, часы, день месяца, месяц, день недели;</item>
/// <item>Quartz из шести или семи полей: секунды, минуты, часы, день месяца, месяц, день недели и необязательный год.</item>
/// </list>
/// Число полей однозначно определяет формат, поэтому выражение из шести и более полей
/// всегда читается по правилам Quartz: там дни недели нумеруются с единицы (1 — воскресенье),
/// а незаданное поле дня записывается вопросительным знаком.
/// </summary>
public static class CronSupport
{
    private static readonly string[] WeekDayNames = ["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"];

    /// <summary>Максимальное значение поля — нужно для развёртывания шага вида «5/10».</summary>
    private static int MaxValue(FieldKind kind) => kind switch
    {
        FieldKind.Second or FieldKind.Minute => 59,
        FieldKind.Hour => 23,
        FieldKind.DayOfMonth => 31,
        FieldKind.Month => 12,
        _ => 6
    };

    private enum FieldKind { Second, Minute, Hour, DayOfMonth, Month, DayOfWeek, QuartzDayOfWeek }

    /// <summary>Выражение записано в формате Quartz (шесть или семь полей).</summary>
    public static bool IsQuartzFormat(string? cron)
    {
        if (string.IsNullOrWhiteSpace(cron)) return false;
        var count = Split(cron).Length;
        return count is 6 or 7;
    }

    /// <summary>Приводит расписание любого поддерживаемого формата к виду для Cronos.</summary>
    /// <exception cref="FormatException">Выражение записано с ошибкой.</exception>
    public static NormalizedCron Normalize(string? cron)
    {
        if (string.IsNullOrWhiteSpace(cron))
            throw new FormatException("Расписание повторения не заполнено.");

        var parts = Split(cron);
        switch (parts.Length)
        {
            case 5:
            {
                var fields = new[]
                {
                    "0",
                    NormalizeField(parts[0], FieldKind.Minute),
                    NormalizeField(parts[1], FieldKind.Hour),
                    NormalizeField(parts[2], FieldKind.DayOfMonth),
                    NormalizeField(parts[3], FieldKind.Month),
                    NormalizeField(parts[4], FieldKind.DayOfWeek)
                };
                return new NormalizedCron(string.Join(' ', fields.Skip(1)), false, false, fields);
            }
            case 6:
            case 7:
            {
                if (parts.Length == 7) EnsureYearIsAny(parts[6]);
                var fields = new[]
                {
                    NormalizeField(parts[0], FieldKind.Second),
                    NormalizeField(parts[1], FieldKind.Minute),
                    NormalizeField(parts[2], FieldKind.Hour),
                    NormalizeField(parts[3], FieldKind.DayOfMonth),
                    NormalizeField(parts[4], FieldKind.Month),
                    NormalizeField(parts[5], FieldKind.QuartzDayOfWeek)
                };
                return new NormalizedCron(string.Join(' ', fields), true, true, fields);
            }
            default:
                throw new FormatException(
                    $"Расписание должно содержать 5 полей (Unix cron) либо 6–7 полей (Quartz). Получено полей: {parts.Length}.");
        }
    }

    /// <summary>Разбирает расписание в готовое к вычислению выражение Cronos.</summary>
    /// <exception cref="FormatException">Выражение записано с ошибкой.</exception>
    public static CronExpression Parse(string? cron)
    {
        var normalized = Normalize(cron);
        try
        {
            return CronExpression.Parse(
                normalized.Expression,
                normalized.IncludeSeconds ? CronFormat.IncludeSeconds : CronFormat.Standard);
        }
        catch (CronFormatException ex)
        {
            throw new FormatException($"Некорректное расписание повторения: {ex.Message}", ex);
        }
    }

    public static bool TryParse(string? cron, out CronExpression? expression, out string? error)
    {
        try
        {
            expression = Parse(cron);
            error = null;
            return true;
        }
        catch (FormatException ex)
        {
            expression = null;
            error = ex.Message;
            return false;
        }
    }

    /// <summary>Возвращает текст ошибки или <c>null</c>, если расписание корректно. Пустое значение считается корректным.</summary>
    public static string? Validate(string? cron)
    {
        if (string.IsNullOrWhiteSpace(cron)) return null;
        return TryParse(cron, out _, out var error) ? null : error;
    }

    private static string[] Split(string cron) =>
        cron.Trim().ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    private static void EnsureYearIsAny(string year)
    {
        if (year is "*" or "?") return;
        throw new FormatException(
            "Поле года в Quartz-выражении не поддерживается: укажите «*» или «?», либо уберите седьмое поле.");
    }

    private static string NormalizeField(string field, FieldKind kind)
    {
        var tokens = field.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) throw new FormatException($"Пустое поле в расписании: «{field}».");
        return string.Join(',', tokens.Select(token => NormalizeToken(token, kind)));
    }

    private static string NormalizeToken(string token, FieldKind kind)
    {
        var slash = token.IndexOf('/');
        var range = slash >= 0 ? token[..slash] : token;
        var step = slash >= 0 ? token[(slash + 1)..] : null;

        range = NormalizeRange(range, kind);
        if (step is null) return range;
        if (step.Length == 0) throw new FormatException($"В расписании не указан шаг: «{token}».");

        // Quartz допускает «5/10» — «каждые 10 начиная с 5». Развёртывание в диапазон
        // даёт тот же смысл и не зависит от того, какие сокращения понимает Cronos.
        if (IsPlainNumber(range)) range = $"{range}-{MaxValue(kind)}";
        return $"{range}/{step}";
    }

    private static string NormalizeRange(string range, FieldKind kind)
    {
        if (range.Length == 0) throw new FormatException("В расписании обнаружено пустое значение.");
        // «?» в Quartz означает «поле не задано» — для Cronos это то же, что «*».
        if (range is "?" or "*") return "*";
        if (kind != FieldKind.QuartzDayOfWeek) return range;

        // Одиночная «L» в поле дня недели Quartz означает субботу, а не последний день.
        if (range == "L") return "6";

        var dash = range.IndexOf('-');
        return dash > 0
            ? $"{ShiftWeekDay(range[..dash])}-{ShiftWeekDay(range[(dash + 1)..])}"
            : ShiftWeekDay(range);
    }

    /// <summary>
    /// В Quartz воскресенье — это 1, в Unix-cron и Cronos — 0. Названия дней в обоих
    /// форматах совпадают, поэтому сдвигаются только числовые значения.
    /// </summary>
    private static string ShiftWeekDay(string value)
    {
        if (value.Length == 0) throw new FormatException("В расписании не указан день недели.");

        var core = value;
        var suffix = string.Empty;
        var hash = core.IndexOf('#');
        if (hash > 0)
        {
            suffix = core[hash..];
            core = core[..hash];
        }
        else if (core.Length > 1 && core[^1] == 'L')
        {
            suffix = "L";
            core = core[..^1];
        }

        if (!int.TryParse(core, NumberStyles.None, CultureInfo.InvariantCulture, out var day))
            return core + suffix;

        if (day is < 1 or > 7)
            throw new FormatException(
                $"День недели в Quartz задаётся числом от 1 (воскресенье) до 7 (суббота). Получено: «{core}».");

        return $"{day - 1}{suffix}";
    }

    private static bool IsPlainNumber(string value) =>
        value.Length > 0 && value.All(char.IsAsciiDigit);

    /// <summary>Номер дня недели по названию или числу в нотации Unix-cron (0 и 7 — воскресенье).</summary>
    internal static int? TryReadWeekDay(string token)
    {
        if (int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
            return number is < 0 or > 7 ? null : number == 7 ? 0 : number;

        var index = Array.IndexOf(WeekDayNames, token.ToUpperInvariant());
        return index < 0 ? null : index;
    }
}
