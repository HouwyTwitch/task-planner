using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Planner.App.ViewModels;
using Planner.Core.Models;

namespace Planner.App.Views;

public partial class CalendarSurface : UserControl
{
    private const double TotalHeight = 1152;
    private const double PixelsPerMinute = TotalHeight / 1440.0;

    private Point _dragStart;
    private Point _dragPointerOffset;
    private TaskItem? _dragTask;
    private MainViewModel? _vm;
    private DragPreviewAdorner? _dragAdorner;
    private AdornerLayer? _dragAdornerLayer;
    private Border? _dropHint;
    private bool _isResizing;
    private double _resizeMinutes;
    private double _resizeSnappedMinutes;
    private bool _initialScrollDone;
    private readonly DispatcherTimer _clock;

    public CalendarSurface()
    {
        InitializeComponent();

        // Линия текущего времени должна двигаться и без обновления данных.
        _clock = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(60) };
        _clock.Tick += (_, _) => { if (IsVisible && _dragTask is null && !_isResizing) Render(); };

        Loaded += (_, _) =>
        {
            Attach();
            ScrollToWorkingHours();
            _clock.Start();
        };
        Unloaded += (_, _) => _clock.Stop();
        DataContextChanged += (_, _) => Attach();
        SizeChanged += (_, _) => Render();

        BuildGrid();
    }

    /// <summary>
    /// Сетка суток начинается с 00:00, поэтому при открытии показываем рабочее время,
    /// а не пустую ночь.
    /// </summary>
    private void ScrollToWorkingHours()
    {
        if (_initialScrollDone) return;
        _initialScrollDone = true;
        var minutes = Math.Clamp(DateTime.Now.TimeOfDay.TotalMinutes - 60, 7 * 60, 20 * 60);
        Dispatcher.InvokeAsync(() => TimeScroll.ScrollToVerticalOffset(minutes * PixelsPerMinute), DispatcherPriority.Loaded);
    }

    private void Attach()
    {
        if (_vm is not null)
        {
            _vm.PropertyChanged -= VmChanged;
            _vm.CalendarTasks.CollectionChanged -= TasksChanged;
            _vm.VisibleDays.CollectionChanged -= TasksChanged;
            _vm.DateOnlyTasks.CollectionChanged -= TasksChanged;
        }

        _vm = DataContext as MainViewModel;
        if (_vm is not null)
        {
            _vm.PropertyChanged += VmChanged;
            _vm.CalendarTasks.CollectionChanged += TasksChanged;
            _vm.VisibleDays.CollectionChanged += TasksChanged;
            _vm.DateOnlyTasks.CollectionChanged += TasksChanged;
        }
        Render();
    }

    private void VmChanged(object? sender, PropertyChangedEventArgs e) => Dispatcher.Invoke(Render);
    private void TasksChanged(object? sender, NotifyCollectionChangedEventArgs e) => Dispatcher.Invoke(Render);

    private void BuildGrid()
    {
        TimeLabels.Children.Clear();
        for (var hour = 0; hour < 24; hour++)
        {
            var y = hour * 48.0;
            var label = new TextBlock { Text = $"{hour:00}:00", FontSize = 11, Foreground = Brushes.Gray };
            Canvas.SetTop(label, y - 7);
            Canvas.SetLeft(label, 8);
            TimeLabels.Children.Add(label);
        }
    }

    private void Render()
    {
        if (!IsLoaded || _vm is null) return;
        TaskCanvas.Children.Clear();
        DateOnlyCanvas.Children.Clear();
        _dropHint = null;

        var days = _vm.VisibleDays.ToList();
        if (days.Count == 0) return;

        var width = Math.Max(100, TaskCanvas.ActualWidth);
        var dayWidth = width / days.Count;
        var counts = days.Select(d => _vm.DateOnlyTasks.Count(t => t.StartDate?.Date == d.Date)).ToList();
        DateOnlyCanvas.Height = Math.Max(42, (counts.Count == 0 ? 0 : counts.Max()) * 42 + 8);
        var dateWidth = Math.Max(100, DateOnlyCanvas.ActualWidth);
        var dateDayWidth = dateWidth / days.Count;

        for (var i = 0; i <= days.Count; i++)
        {
            var line = new Border { Width = 1, Height = TotalHeight, Background = new SolidColorBrush(Color.FromRgb(225, 225, 225)) };
            Canvas.SetLeft(line, i * dayWidth);
            TaskCanvas.Children.Add(line);

            var topLine = new Border { Width = 1, Height = Math.Max(1, DateOnlyCanvas.ActualHeight), Background = new SolidColorBrush(Color.FromRgb(225, 225, 225)) };
            Canvas.SetLeft(topLine, i * dateDayWidth);
            DateOnlyCanvas.Children.Add(topLine);
        }

        for (var half = 0; half <= 48; half++)
        {
            var line = new Border
            {
                Height = 1,
                Width = width,
                Background = new SolidColorBrush(half % 2 == 0 ? Color.FromRgb(225, 225, 225) : Color.FromRgb(242, 242, 242))
            };
            Canvas.SetTop(line, half * 24);
            TaskCanvas.Children.Add(line);
        }

        var noTimeRows = new Dictionary<int, int>();
        foreach (var task in _vm.DateOnlyTasks)
        {
            if (task.StartDate is null) continue;
            var dayIndex = days.FindIndex(d => d.Date == task.StartDate.Value.Date);
            if (dayIndex < 0) continue;
            var row = noTimeRows.TryGetValue(dayIndex, out var r) ? r : 0;
            noTimeRows[dayIndex] = row + 1;
            var border = CreateTaskBorder(
                task,
                Math.Max(30, dateDayWidth - 6),
                38,
                _vm.CanManageTask(task),
                false,
                null,
                false);
            Canvas.SetLeft(border, dayIndex * dateDayWidth + 3);
            Canvas.SetTop(border, 4 + row * 42);
            DateOnlyCanvas.Children.Add(border);
        }

        foreach (var item in _vm.CalendarTasks)
        {
            if (item.Task.StartDate is null) continue;
            var dayIndex = days.FindIndex(d => d.Date == item.Start.Date);
            if (dayIndex < 0) continue;
            var minutes = item.Start.TimeOfDay.TotalMinutes;
            var top = minutes * PixelsPerMinute;
            var logicalHeight = DurationPixels(item.Task.EffectiveDuration.TotalMinutes);
            var canManage = _vm.CanManageTask(item.Task);
            var border = CreateTaskBorder(
                item.Task,
                Math.Max(30, dayWidth - 6),
                logicalHeight,
                canManage,
                canManage,
                $"{item.Start:HH:mm}–{item.End:HH:mm}",
                true);
            Canvas.SetLeft(border, dayIndex * dayWidth + 3);
            Canvas.SetTop(border, top);
            Panel.SetZIndex(border, 10);
            TaskCanvas.Children.Add(border);
        }

        RenderNowLine(days, dayWidth);
    }

    /// <summary>Красная линия текущего времени в колонке сегодняшнего дня — как в Google Календаре.</summary>
    private void RenderNowLine(List<DateTime> days, double dayWidth)
    {
        var todayIndex = days.FindIndex(d => d.Date == DateTime.Today);
        if (todayIndex < 0) return;

        var accent = new SolidColorBrush(Color.FromRgb(217, 48, 37));
        var y = DateTime.Now.TimeOfDay.TotalMinutes * PixelsPerMinute;

        var line = new Border { Height = 2, Width = dayWidth, Background = accent, IsHitTestVisible = false };
        Canvas.SetLeft(line, todayIndex * dayWidth);
        Canvas.SetTop(line, y - 1);
        Panel.SetZIndex(line, 40);
        TaskCanvas.Children.Add(line);

        var dot = new Border
        {
            Width = 10,
            Height = 10,
            CornerRadius = new CornerRadius(5),
            Background = accent,
            IsHitTestVisible = false
        };
        Canvas.SetLeft(dot, todayIndex * dayWidth - 4);
        Canvas.SetTop(dot, y - 5);
        Panel.SetZIndex(dot, 41);
        TaskCanvas.Children.Add(dot);
    }

    private Border CreateTaskBorder(
        TaskItem task,
        double width,
        double logicalHeight,
        bool canDrag,
        bool canResize,
        string? timeText,
        bool allowContentExpansion)
    {
        const double resizeGripHeight = 22;
        var colors = GetTaskColors(task);

        var border = new Border
        {
            DataContext = task,
            Background = new SolidColorBrush(colors.Background),
            BorderBrush = new SolidColorBrush(colors.Border),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(6, 4, 6, 4),
            Width = width,
            ToolTip = BuildToolTip(task),
            ContextMenu = BuildContextMenu(task),
            ClipToBounds = true
        };

        var title = new TextBlock
        {
            Text = task.Title,
            Foreground = Brushes.White,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.None,
            IsHitTestVisible = false
        };

        var metaText = string.IsNullOrWhiteSpace(timeText)
            ? $"Исполнитель: {task.AssigneeDisplay} · {task.StatusDisplay}"
            : $"{timeText} · {task.StatusDisplay}\nИсполнитель: {task.AssigneeDisplay}";

        var meta = new TextBlock
        {
            Text = metaText,
            Foreground = new SolidColorBrush(Color.FromArgb(235, 255, 255, 255)),
            FontSize = 9.5,
            Margin = new Thickness(0, 2, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.None,
            IsHitTestVisible = false
        };

        var content = new StackPanel();
        content.Children.Add(title);
        content.Children.Add(meta);
        content.Measure(new Size(Math.Max(24, width - 12), double.PositiveInfinity));

        var readableHeight = content.DesiredSize.Height + 10;
        var baseReadableHeight = allowContentExpansion
            ? Math.Max(46, readableHeight + (canResize ? resizeGripHeight : 0))
            : logicalHeight;
        border.Height = Math.Max(baseReadableHeight, logicalHeight);

        var root = new Grid();
        root.Children.Add(content);

        if (canResize)
        {
            // Тонкая линия показывает фактический конец задачи, даже если карточка
            // визуально увеличена ради полного текста.
            var durationLine = new Border
            {
                Height = 2,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(Color.FromArgb(185, 255, 255, 255)),
                IsHitTestVisible = false
            };
            Panel.SetZIndex(durationLine, 18);
            root.Children.Add(durationLine);

            var resizeLabel = new TextBlock
            {
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0)),
                Padding = new Thickness(5, 2, 5, 2),
                Margin = new Thickness(0, 0, 6, resizeGripHeight + 2),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                FontSize = 10,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false
            };
            Panel.SetZIndex(resizeLabel, 19);
            root.Children.Add(resizeLabel);

            void PositionDurationGuide(double durationHeight)
            {
                border.Height = Math.Max(baseReadableHeight, durationHeight + resizeGripHeight / 2.0);
                durationLine.Margin = new Thickness(0, Math.Max(0, durationHeight - 1), 0, 0);
            }

            var resizeThumb = new Thumb
            {
                Height = resizeGripHeight,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Cursor = Cursors.SizeNS,
                Background = Brushes.Transparent,
                ToolTip = "Изменить длительность: тяните широкую нижнюю область вверх или вниз"
            };
            Panel.SetZIndex(resizeThumb, 20);
            resizeThumb.Template = CreateResizeThumbTemplate();
            PositionDurationGuide(logicalHeight);

            resizeThumb.DragStarted += (_, e) =>
            {
                _isResizing = true;
                _resizeMinutes = task.EffectiveDuration.TotalMinutes;
                _resizeSnappedMinutes = _resizeMinutes;
                resizeLabel.Text = FormatDuration(_resizeSnappedMinutes);
                resizeLabel.Visibility = Visibility.Visible;
                e.Handled = true;
            };
            resizeThumb.DragDelta += (_, e) =>
            {
                if (_vm is null) return;
                _resizeMinutes += e.VerticalChange / PixelsPerMinute;
                var snap = Math.Max(1, _vm.SnapMinutes);
                _resizeSnappedMinutes = Math.Max(snap, Math.Round(_resizeMinutes / snap) * snap);

                var newLogicalHeight = DurationPixels(_resizeSnappedMinutes);
                PositionDurationGuide(newLogicalHeight);
                resizeLabel.Text = FormatDuration(_resizeSnappedMinutes);
                e.Handled = true;
            };
            resizeThumb.DragCompleted += async (_, e) =>
            {
                resizeLabel.Visibility = Visibility.Collapsed;
                try
                {
                    if (_vm is not null)
                        await _vm.ResizeTaskAsync(task, TimeSpan.FromMinutes(_resizeSnappedMinutes));
                }
                finally
                {
                    _isResizing = false;
                }
                e.Handled = true;
            };
            root.Children.Add(resizeThumb);
        }

        border.Child = root;
        border.PreviewMouseLeftButtonDown += Task_MouseDown;
        border.Cursor = Cursors.Hand;
        if (canDrag)
        {
            border.MouseMove += Task_MouseMove;
            border.GiveFeedback += Task_GiveFeedback;
        }
        return border;
    }

    private static string BuildToolTip(TaskItem task)
    {
        var lines = new List<string>
        {
            task.Title,
            $"Время: {(string.IsNullOrEmpty(task.TimeDisplay) ? "—" : task.TimeDisplay)}",
            $"Исполнитель: {task.AssigneeDisplay}",
            $"Статус: {task.StatusDisplay}",
            $"Создал: {task.CreatorDisplay}"
        };
        if (!string.IsNullOrWhiteSpace(task.Description)) lines.Add(task.Description!.Trim());
        lines.Add("Двойной щелчок — открыть задачу");
        return string.Join(Environment.NewLine, lines);
    }

    private static ControlTemplate CreateResizeThumbTemplate()
    {
        var template = new ControlTemplate(typeof(Thumb));

        var hitArea = new FrameworkElementFactory(typeof(Border));
        hitArea.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(42, 255, 255, 255)));
        hitArea.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
        hitArea.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
        hitArea.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Stretch);

        var handle = new FrameworkElementFactory(typeof(Border));
        handle.SetValue(Border.HeightProperty, 4.0);
        handle.SetValue(Border.WidthProperty, 58.0);
        handle.SetValue(Border.CornerRadiusProperty, new CornerRadius(2));
        handle.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(245, 255, 255, 255)));
        handle.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Center);
        handle.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        hitArea.AppendChild(handle);

        template.VisualTree = hitArea;
        return template;
    }

    private static string FormatDuration(double minutes)
    {
        var total = Math.Max(0, (int)Math.Round(minutes));
        var hours = total / 60;
        var mins = total % 60;
        if (hours == 0) return $"{mins} мин";
        if (mins == 0) return $"{hours} ч";
        return $"{hours} ч {mins} мин";
    }

    private static (Color Background, Color Border) GetTaskColors(TaskItem task) => task.VisualState switch
    {
        TaskVisualStates.InProgress => (Color.FromRgb(24, 128, 56), Color.FromRgb(13, 101, 45)),
        TaskVisualStates.Overdue => (Color.FromRgb(217, 48, 37), Color.FromRgb(165, 14, 14)),
        TaskVisualStates.Completed => (Color.FromRgb(128, 134, 139), Color.FromRgb(95, 99, 104)),
        _ => (Color.FromRgb(26, 115, 232), Color.FromRgb(21, 88, 176))
    };

    private ContextMenu BuildContextMenu(TaskItem task)
    {
        var menu = new ContextMenu();
        var open = new MenuItem { Header = "Открыть задачу", FontWeight = FontWeights.SemiBold };
        open.Click += async (_, _) => { if (_vm is not null) await _vm.EditSpecificTaskAsync(task); };
        menu.Items.Add(open);

        var status = new MenuItem { Header = "Изменить статус" };
        AddStatus(status, "Ожидает выполнения", TaskStatuses.Pending, task);
        AddStatus(status, "В работе", TaskStatuses.InProgress, task);
        AddStatus(status, "Выполнено", TaskStatuses.Completed, task);
        menu.Items.Add(status);

        if (_vm?.CanManageTask(task) == true)
        {
            var transfer = new MenuItem { Header = "Передать / назначить исполнителя" };
            var targets = _vm.GetTransferTargets(task);
            if (targets.Count == 0) transfer.IsEnabled = false;
            foreach (var user in targets)
            {
                var mi = new MenuItem { Header = user.DisplayName, Tag = user };
                mi.Click += async (_, _) => { if (_vm is not null) await _vm.TransferTaskAsync(task, user); };
                transfer.Items.Add(mi);
            }
            menu.Items.Add(transfer);
            menu.Items.Add(new Separator());
            var delete = new MenuItem { Header = "Удалить задачу" };
            delete.Click += async (_, _) => { if (_vm is not null) await _vm.DeleteTaskAsync(task); };
            menu.Items.Add(delete);
        }
        return menu;
    }

    private void AddStatus(MenuItem parent, string caption, string code, TaskItem task)
    {
        var item = new MenuItem { Header = caption, IsCheckable = true, IsChecked = task.Status == code };
        item.Click += async (_, _) => { if (_vm is not null) await _vm.ChangeStatusAsync(task, code); };
        parent.Items.Add(item);
    }

    private void Task_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border border || border.DataContext is not TaskItem task) return;
        _vm?.SelectTask(task);
        if (FindAncestor<Thumb>(e.OriginalSource as DependencyObject) is not null) return;

        if (e.ClickCount >= 2)
        {
            // Двойной щелчок по карточке открывает задачу. Перетаскивание при этом не начинается.
            _dragTask = null;
            e.Handled = true;
            OpenTask(task);
            return;
        }

        if (_isResizing || _vm?.CanManageTask(task) != true) return;
        _dragTask = task;
        _dragStart = e.GetPosition(this);
        _dragPointerOffset = e.GetPosition(border);
    }

    /// <summary>
    /// Модальное окно задачи открывается после того, как WPF закончит обработку щелчка:
    /// иначе диалог появляется при захваченной мыши и второй щелчок «прилипает» к карточке.
    /// </summary>
    private void OpenTask(TaskItem task)
    {
        var vm = _vm;
        if (vm is null) return;
        Dispatcher.InvokeAsync(new Action(() => OpenTaskCore(vm, task)), DispatcherPriority.Input);
    }

    private static async void OpenTaskCore(MainViewModel vm, TaskItem task) => await vm.EditSpecificTaskAsync(task);

    private void Task_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isResizing || e.LeftButton != MouseButtonState.Pressed || _dragTask is null || sender is not Border source) return;
        var p = e.GetPosition(this);
        if (Math.Abs(p.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(p.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance) return;

        StartDragPreview(source);
        try
        {
            DragDrop.DoDragDrop(source, new DataObject(typeof(TaskItem), _dragTask), DragDropEffects.Move);
        }
        finally
        {
            StopDragPreview();
            RemoveDropHint();
            _dragTask = null;
        }
    }

    private void StartDragPreview(Border source)
    {
        StopDragPreview();
        _dragAdornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (_dragAdornerLayer is null) return;
        _dragAdorner = new DragPreviewAdorner(this, source)
        {
            Position = new Point(_dragStart.X - _dragPointerOffset.X, _dragStart.Y - _dragPointerOffset.Y)
        };
        _dragAdornerLayer.Add(_dragAdorner);
    }

    private void StopDragPreview()
    {
        if (_dragAdorner is not null && _dragAdornerLayer is not null)
            _dragAdornerLayer.Remove(_dragAdorner);
        _dragAdorner = null;
        _dragAdornerLayer = null;
    }

    private void CalendarSurface_PreviewDragOver(object sender, DragEventArgs e)
    {
        if (_dragAdorner is null) return;
        var mouse = e.GetPosition(this);
        _dragAdorner.Position = new Point(mouse.X - _dragPointerOffset.X, mouse.Y - _dragPointerOffset.Y);
        _dragAdorner.InvalidateVisual();
    }

    private void Task_GiveFeedback(object sender, GiveFeedbackEventArgs e)
    {
        Mouse.SetCursor(Cursors.Hand);
        if (_dragAdorner is not null)
        {
            var mouse = Mouse.GetPosition(this);
            _dragAdorner.Position = new Point(mouse.X - _dragPointerOffset.X, mouse.Y - _dragPointerOffset.Y);
            _dragAdorner.InvalidateVisual();
        }
        e.Handled = true;
    }

    private async void TaskCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2 || _vm is null || FindTaskBorder(e.OriginalSource as DependencyObject) is not null) return;
        var start = ResolveGridDateTime(e.GetPosition(TaskCanvas));
        if (start is null) return;
        e.Handled = true;
        await _vm.CreateTaskAtAsync(start.Value);
    }

    private void TaskCanvas_DragOver(object sender, DragEventArgs e)
    {
        if (_vm is null || !e.Data.GetDataPresent(typeof(TaskItem)) || e.Data.GetData(typeof(TaskItem)) is not TaskItem task)
        {
            e.Effects = DragDropEffects.None;
            return;
        }

        var target = ResolveGridDateTime(e.GetPosition(TaskCanvas));
        if (target is null) return;
        ShowDropHint(task, target.Value);
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void TaskCanvas_DragLeave(object sender, DragEventArgs e) => RemoveDropHint();

    private async void TaskCanvas_Drop(object sender, DragEventArgs e)
    {
        RemoveDropHint();
        if (_vm is null || !e.Data.GetDataPresent(typeof(TaskItem)) || e.Data.GetData(typeof(TaskItem)) is not TaskItem task) return;
        var start = ResolveGridDateTime(e.GetPosition(TaskCanvas));
        if (start is null) return;
        e.Handled = true;
        await _vm.RescheduleTaskAsync(task, start.Value);
    }

    private DateTime? ResolveGridDateTime(Point p)
    {
        if (_vm is null) return null;
        var days = _vm.VisibleDays.ToList();
        if (days.Count == 0) return null;
        var dayWidth = Math.Max(1, TaskCanvas.ActualWidth / days.Count);
        var index = Math.Clamp((int)(p.X / dayWidth), 0, days.Count - 1);
        var rawMinutes = Math.Clamp(p.Y / TotalHeight * 1440.0, 0, 1439);
        var snap = Math.Max(1, _vm.SnapMinutes);
        var rounded = Math.Round(rawMinutes / snap) * snap;
        rounded = Math.Min(1439, rounded);
        return days[index].Date.AddMinutes(rounded);
    }

    private void ShowDropHint(TaskItem task, DateTime start)
    {
        if (_vm is null) return;
        var days = _vm.VisibleDays.ToList();
        var dayIndex = days.FindIndex(d => d.Date == start.Date);
        if (dayIndex < 0) return;
        var dayWidth = Math.Max(1, TaskCanvas.ActualWidth / days.Count);
        var colors = GetTaskColors(task);
        _dropHint ??= new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(65, colors.Background.R, colors.Background.G, colors.Background.B)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(210, colors.Border.R, colors.Border.G, colors.Border.B)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(5),
            IsHitTestVisible = false
        };
        if (!TaskCanvas.Children.Contains(_dropHint)) TaskCanvas.Children.Add(_dropHint);
        _dropHint.Width = Math.Max(30, dayWidth - 6);
        _dropHint.Height = DurationPixels(task.IsAllDay ? 60 : task.EffectiveDuration.TotalMinutes);
        Canvas.SetLeft(_dropHint, dayIndex * dayWidth + 3);
        Canvas.SetTop(_dropHint, start.TimeOfDay.TotalMinutes * PixelsPerMinute);
        Panel.SetZIndex(_dropHint, 50);
    }

    private void RemoveDropHint()
    {
        if (_dropHint is not null && TaskCanvas.Children.Contains(_dropHint)) TaskCanvas.Children.Remove(_dropHint);
        _dropHint = null;
    }

    private static Border? FindTaskBorder(DependencyObject? source)
    {
        var current = source;
        while (current is not null)
        {
            if (current is Border border && border.DataContext is TaskItem) return border;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private static T? FindAncestor<T>(DependencyObject? source) where T : DependencyObject
    {
        var current = source;
        while (current is not null)
        {
            if (current is T match) return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private static double DurationPixels(double minutes) => Math.Max(4, minutes * PixelsPerMinute);

    private sealed class DragPreviewAdorner : Adorner
    {
        private readonly Brush _brush;
        private readonly Size _size;
        public Point Position { get; set; }

        public DragPreviewAdorner(UIElement adornedElement, FrameworkElement source) : base(adornedElement)
        {
            IsHitTestVisible = false;
            _size = source.RenderSize;
            _brush = new VisualBrush(source) { Opacity = 0.82, Stretch = Stretch.Fill };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawRoundedRectangle(_brush, null, new Rect(Position, _size), 5, 5);
        }
    }
}
