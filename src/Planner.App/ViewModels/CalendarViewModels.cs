using System.Collections.ObjectModel;
using Planner.App.Infrastructure;
using Planner.Core.Models;

namespace Planner.App.ViewModels;

public sealed class CalendarTaskViewModel : ObservableObject
{
    public TaskItem Task { get; }
    public CalendarTaskViewModel(TaskItem task)=>Task=task;
    public long Id=>Task.Id; public string Title=>Task.Title; public DateTime Start=>Task.StartDate??DateTime.MinValue; public DateTime End=>Task.EndDate??Start.AddHours(1);
}

public sealed class MonthDayViewModel
{
    public DateTime Date { get; init; }
    public bool IsCurrentMonth { get; init; }
    public bool IsToday => Date.Date==DateTime.Today;
    public ObservableCollection<TaskItem> Tasks { get; }=new();
}
