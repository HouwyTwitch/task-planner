using System.Collections.ObjectModel;
using Planner.App.Infrastructure;
using Planner.Core.Models;

namespace Planner.App.ViewModels;

/// <summary>
/// Отчёт руководителя: сводка по себе и подчинённым плюс общая строка.
/// Данные загружает делегат, поэтому окно не знает о слое доступа к данным.
/// </summary>
public sealed class StatisticsViewModel : ObservableObject
{
    public const string PeriodWeek = "Текущая неделя";
    public const string PeriodMonth = "Текущий месяц";
    public const string PeriodQuarter = "Текущий квартал";
    public const string PeriodYear = "Текущий год";
    public const string PeriodCustom = "Произвольный период";

    private readonly Func<DateTime, DateTime, CancellationToken, Task<TaskStatisticsReport>> _load;

    public IReadOnlyList<string> Periods { get; } = [PeriodWeek, PeriodMonth, PeriodQuarter, PeriodYear, PeriodCustom];
    public ObservableCollection<TaskStatisticsRow> Rows { get; } = new();

    private string _selectedPeriod = PeriodMonth;
    public string SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (!Set(ref _selectedPeriod, value)) return;
            Raise(nameof(IsCustomPeriod));
            if (!IsCustomPeriod) ApplyPeriod(value);
            _ = ReloadAsync();
        }
    }

    public bool IsCustomPeriod => SelectedPeriod == PeriodCustom;

    private DateTime _from = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    public DateTime From { get => _from; set { if (Set(ref _from, value.Date)) Raise(nameof(PeriodCaption)); } }

    private DateTime _to = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);
    public DateTime To { get => _to; set { if (Set(ref _to, value.Date)) Raise(nameof(PeriodCaption)); } }

    private TaskStatisticsRow _total = new() { DisplayName = "Итого", IsTotal = true };
    public TaskStatisticsRow Total { get => _total; private set => Set(ref _total, value); }

    private bool _isBusy;
    public bool IsBusy { get => _isBusy; private set { if (Set(ref _isBusy, value)) RefreshCommand.Raise(); } }

    private string? _errorText;
    public string? ErrorText { get => _errorText; private set { if (Set(ref _errorText, value)) Raise(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorText);

    public string ScopeCaption { get; }
    public string PeriodCaption => $"Период: {From:dd.MM.yyyy} — {To:dd.MM.yyyy}";
    public string EmployeesCaption => $"Сотрудников в отчёте: {Rows.Count}";

    public AsyncRelayCommand RefreshCommand { get; }

    public StatisticsViewModel(User loggedUser, Func<DateTime, DateTime, CancellationToken, Task<TaskStatisticsReport>> load)
    {
        _load = load;
        ScopeCaption = loggedUser.Role == UserRoles.Admin
            ? "Все сотрудники организации"
            : $"{loggedUser.DisplayName} и подчинённые";
        RefreshCommand = new AsyncRelayCommand(() => ReloadAsync(), () => !IsBusy);
        ApplyPeriod(_selectedPeriod);
    }

    public async Task ReloadAsync(CancellationToken ct = default)
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorText = null;
        try
        {
            var from = From.Date;
            var to = To.Date.AddDays(1);
            if (to <= from)
            {
                ErrorText = "Конец периода должен быть не раньше его начала.";
                Rows.Clear();
                Total = new TaskStatisticsRow { DisplayName = "Итого", IsTotal = true };
                return;
            }

            var report = await _load(from, to, ct);
            Rows.Clear();
            foreach (var row in report.Rows) Rows.Add(row);
            Total = report.Total;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            ErrorText = ex.Message;
        }
        finally
        {
            IsBusy = false;
            Raise(nameof(PeriodCaption));
            Raise(nameof(EmployeesCaption));
        }
    }

    private void ApplyPeriod(string period)
    {
        var today = DateTime.Today;
        switch (period)
        {
            case PeriodWeek:
                var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
                From = monday;
                To = monday.AddDays(6);
                break;
            case PeriodQuarter:
                var quarterMonth = (today.Month - 1) / 3 * 3 + 1;
                From = new DateTime(today.Year, quarterMonth, 1);
                To = From.AddMonths(3).AddDays(-1);
                break;
            case PeriodYear:
                From = new DateTime(today.Year, 1, 1);
                To = new DateTime(today.Year, 12, 31);
                break;
            default:
                From = new DateTime(today.Year, today.Month, 1);
                To = From.AddMonths(1).AddDays(-1);
                break;
        }
    }

    /// <summary>Отчёт в CSV с разделителем «;» — открывается в Excel без настройки импорта.</summary>
    public string BuildCsv()
    {
        var lines = new List<string>
        {
            $"Отчёт по задачам;{From:dd.MM.yyyy};{To:dd.MM.yyyy}",
            ScopeCaption,
            string.Empty,
            "Сотрудник;Всего;Ожидает;В работе;Выполнено;Просрочено;% выполнения;Свои;Поручено"
        };
        foreach (var row in Rows) lines.Add(FormatCsvRow(row));
        lines.Add(FormatCsvRow(Total));
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatCsvRow(TaskStatisticsRow row) =>
        string.Join(';',
            Escape(row.DisplayName), row.Total, row.Pending, row.InProgress, row.Completed,
            row.Overdue, row.CompletionRateDisplay, row.Own, row.Delegated);

    private static string Escape(string value) => value.Replace(';', ',');
}
