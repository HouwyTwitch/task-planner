namespace Planner.Core.Services;

/// <summary>
/// Правила переноса задач с выходных дней на будние.
/// Рабочая неделя — с понедельника по пятницу.
/// </summary>
public static class WorkdayCalculator
{
    public static bool IsWeekend(DateTime date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>
    /// Подбирает будний день для задачи, выпавшей на выходной.
    /// Сначала берётся ближайший будний день перед выходным: работу лучше сделать заранее.
    /// Если этот день уже прошёл или наступает не позже <paramref name="notBefore"/>,
    /// переносить назад бессмысленно, и выбирается ближайший будний день после выходного.
    /// Время суток сохраняется.
    /// </summary>
    /// <param name="date">Исходная дата задачи.</param>
    /// <param name="notBefore">День, позже которого должен оказаться перенос: обычно сегодняшняя дата.</param>
    public static DateTime ShiftFromWeekend(DateTime date, DateTime notBefore)
    {
        if (!IsWeekend(date)) return date;

        var backward = date;
        for (var step = 0; step < 7 && IsWeekend(backward); step++) backward = backward.AddDays(-1);
        if (!IsWeekend(backward) && backward.Date > notBefore.Date) return backward;

        var forward = date;
        for (var step = 0; step < 7 && IsWeekend(forward); step++) forward = forward.AddDays(1);
        return IsWeekend(forward) ? date : forward;
    }
}
