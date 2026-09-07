using System.Globalization;

namespace Planner.Data.Database;

internal static class DbTime
{
    private const string Format = "yyyy-MM-dd HH:mm:ss";

    public static string ToText(DateTime value) => value.ToString(Format, CultureInfo.InvariantCulture);
    public static string? ToText(DateTime? value) => value is null ? null : ToText(value.Value);
    public static DateTime Parse(string value) => DateTime.ParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None);
    public static DateTime? ParseNullable(object? value) => value is null or DBNull ? null : Parse(Convert.ToString(value, CultureInfo.InvariantCulture)!);
}
