namespace Planner.Core.Services;

public static class SimpleCronBuilder
{
    public static string Daily(TimeOnly time) => $"{time.Minute} {time.Hour} * * *";

    public static string Weekly(DayOfWeek day, TimeOnly time)
    {
        var cronDay = day == DayOfWeek.Sunday ? 0 : (int)day;
        return $"{time.Minute} {time.Hour} * * {cronDay}";
    }

    public static string Weekdays(TimeOnly time) => $"{time.Minute} {time.Hour} * * 1-5";

    public static string Monthly(int dayOfMonth, TimeOnly time) =>
        $"{time.Minute} {time.Hour} {Math.Clamp(dayOfMonth, 1, 31)} * *";

    public static string Quarterly(DateTime firstDate, TimeOnly time) =>
        $"{time.Minute} {time.Hour} {firstDate.Day} {BuildMonthList(firstDate.Month, 3)} *";

    public static string SemiAnnual(DateTime firstDate, TimeOnly time) =>
        $"{time.Minute} {time.Hour} {firstDate.Day} {BuildMonthList(firstDate.Month, 6)} *";

    public static string Annual(DateTime firstDate, TimeOnly time) =>
        $"{time.Minute} {time.Hour} {firstDate.Day} {firstDate.Month} *";

    private static string BuildMonthList(int firstMonth, int step)
    {
        var months = new List<int>();
        for (var month = firstMonth; month <= 12; month += step) months.Add(month);
        for (var month = firstMonth - step; month >= 1; month -= step) months.Add(month);
        return string.Join(',', months.OrderBy(x => x));
    }
}
