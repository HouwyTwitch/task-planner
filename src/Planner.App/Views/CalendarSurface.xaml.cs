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
    private bool _renderPending;
    private readonly DispatcherTimer _clock;

    // Создание задачи протягиванием, как в Google Календаре.
    private bool _creating;
    private bool _createMoved;
    private int _createDayIndex;
    private double _createAnchorMinutes;
    private double _createStartMinutes;
    private double _createEndMinutes;
    private Point _createPressPoint;
    private Border? _createPreview;
    private TextBlock? _createPreviewTime;
    private readonly DispatcherTimer _autoScroll;

    private static readonly Brush DayLineBrush = Frozen(new SolidColorBrush(Color.FromRgb(225, 225, 225)));
    private static readonly Brush HalfHourLineBrush = Frozen(new SolidColorBrush(Color.FromRgb(242, 242, 242)));
    private static readonly Brush MetaBrush = Frozen(new SolidColorBrush(Color.FromArgb(235, 255, 255, 255)));
    private static readonly Brush NowBrush = Frozen(new SolidColorBrush(Color.FromRgb(217, 48, 37)));

    public CalendarSurface()
    {
        InitializeComponent();

        // Линия текущего времени должна двигаться и без обновления данных.
        _clock = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(60) };
        _clock.Tick += (_, _) => { if (IsVisible && _dragTask is null && !_isResizing) ScheduleRender(); };

        // Пока пользователь тянет новую задачу к краю области, сетка прокручивается сама.
        _autoScroll = new DispatcherTimer(DispatcherPriority.Input) { Interval = TimeSpan.FromMilliseconds(30) };
        _autoScroll.Tick += (_, _) => AutoScrollWhileCreating();

        Loaded += (_, _) =>
        {
            Attach();
            ScrollToWorkingHours();
            _clock.Start();
        };
        Unloaded += (_, _) => { _clock.Stop(); CancelCreate(); };
        DataContextChanged += (_, _) => Attach();
        SizeChanged += (_, _) => ScheduleRender();
        IsVisibleChanged += (_, _) => { if (IsVisible) ScheduleRender(); };

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
        ScheduleRender();
    }

    // Перерисовка зависит только от коллекций; смена текста статуса, поиска или выделения
    // не должна пересобирать всю сетку.
    private void VmChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.ViewMode) or nameof(MainViewModel.AnchorDate)) ScheduleRender();
    }

    private void TasksChanged(object? sender, NotifyCollectionChangedEventArgs e) => ScheduleRender();

    /// <summary>
    /// Обновление данных очищает и заново заполняет коллекции: это десятки событий подряд.
    /// Раньше каждое событие синхронно пересобирало весь календарь (O(N²) по числу задач),
    /// из-за чего окно подвисало после создания задачи. Теперь все события сливаются
    /// в одну перерисовку после того, как модель закончит обновление.
    /// </summary>
    private void ScheduleRender()
    {
        if (_renderPending) return;
        _renderPending = true;
        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
        {
            _renderPending = false;
            Render();
        }));
    }

    private static T Frozen<T>(T freezable) where T : Freezable
    {
        freezable.Freeze();
        return freezable;
    }

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
        if (!IsLoaded || !IsVisible || _vm is null) return;
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
            var line = new Border { Width = 1, Height = TotalHeight, Background = DayLineBrush, IsHitTestVisible = false };
            Canvas.SetLeft(line, i * dayWidth);
            TaskCanvas.Children.Add(line);

            var topLine = new Border { Width = 1, Height = Math.Max(1, DateOnlyCanvas.Height), Background = DayLineBrush, IsHitTestVisible = false };
            Canvas.SetLeft(topLine, i * dateDayWidth);
            DateOnlyCanvas.Children.Add(topLine);
        }

        for (var half = 0; half <= 48; half++)
        {
            var line = new Border
            {
                Height = 1,
                Width = width,
                Background = half % 2 == 0 ? DayLineBrush : HalfHourLineBrush,
                IsHitTestVisible = false
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

        var accent = NowBrush;
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
            // Меню строится при открытии, а не для каждой карточки при каждой перерисовке.
            ContextMenu = new ContextMenu(),
            ClipToBounds = true
        };
        border.ContextMenuOpening += (_, _) => border.ContextMenu = BuildContextMenu(task);

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
            Foreground = MetaBrush,
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

    // ───────────── Создание задачи протягиванием (как в Google Календаре) ─────────────
    // Нажатие на пустую ячейку сразу показывает блок с временем начала. Не отпуская кнопку,
    // можно тянуть вниз или вверх — блок растёт с шагом SnapMinutes из настроек.
    // Простой щелчок создаёт задачу длительностью, выбранной в панели инструментов.

    private void TaskCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_vm is null || _creating || FindTaskBorder(e.OriginalSource as DependencyObject) is not null) return;
        var days = _vm.VisibleDays.Count;
        if (days == 0) return;

        var p = e.GetPosition(TaskCanvas);
        var snap = Snap;
        var dayWidth = Math.Max(1, TaskCanvas.ActualWidth / days);
        _createDayIndex = Math.Clamp((int)(p.X / dayWidth), 0, days - 1);
        var raw = Math.Clamp(p.Y / PixelsPerMinute, 0, 1440 - snap);
        _createAnchorMinutes = Math.Floor(raw / snap) * snap;
        _createStartMinutes = _createAnchorMinutes;
        _createEndMinutes = Math.Min(1440, _createAnchorMinutes + Math.Max(snap, _vm.SelectedNewTaskDurationMinutes));
        _createPressPoint = p;
        _createMoved = false;
        _creating = true;

        Focus();
        TaskCanvas.CaptureMouse();
        UpdateCreatePreview();
        _autoScroll.Start();
        e.Handled = true;
    }

    private void TaskCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_creating) return;
        if (e.LeftButton != MouseButtonState.Pressed) { CancelCreate(); return; }
        UpdateCreateRange(e.GetPosition(TaskCanvas));
    }

    private void TaskCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_creating) return;
        e.Handled = true;
        _creating = false;
        _autoScroll.Stop();
        TaskCanvas.ReleaseMouseCapture();

        if (_vm is null || _createDayIndex >= _vm.VisibleDays.Count) { RemoveCreatePreview(); return; }
        var start = _vm.VisibleDays[_createDayIndex].Date.AddMinutes(_createStartMinutes);
        var duration = (int)Math.Round(_createEndMinutes - _createStartMinutes);
        var vm = _vm;

        // Окно задачи открываем после завершения обработки мыши; блок остаётся на сетке,
        // пока окно открыто, — видно, куда попадёт задача.
        Dispatcher.InvokeAsync(new Action(async () =>
        {
            try { await vm.CreateTaskAtAsync(start, duration); }
            finally { RemoveCreatePreview(); }
        }), DispatcherPriority.Input);
    }

    private void TaskCanvas_LostMouseCapture(object sender, MouseEventArgs e)
    {
        // Захват мыши потерян не нами (Alt+Tab, всплывающее окно) — отменяем создание.
        if (_creating) CancelCreate();
    }

    private void CalendarSurface_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && _creating)
        {
            CancelCreate();
            e.Handled = true;
        }
    }

    private void DateOnlyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_vm is null || FindTaskBorder(e.OriginalSource as DependencyObject) is not null) return;
        var days = _vm.VisibleDays.Count;
        if (days == 0) return;
        var dayWidth = Math.Max(1, DateOnlyCanvas.ActualWidth / days);
        var index = Math.Clamp((int)(e.GetPosition(DateOnlyCanvas).X / dayWidth), 0, days - 1);
        var date = _vm.VisibleDays[index].Date;
        var vm = _vm;
        e.Handled = true;
        Dispatcher.InvokeAsync(new Action(async () => await vm.CreateDateOnlyTaskAsync(date)), DispatcherPriority.Input);
    }

    private double Snap => Math.Clamp(_vm?.SnapMinutes ?? 30, 1, 240);

    private void UpdateCreateRange(Point p)
    {
        if (!_createMoved && Math.Abs(p.Y - _createPressPoint.Y) < SystemParameters.MinimumVerticalDragDistance + 2) return;
        _createMoved = true;

        var snap = Snap;
        var current = Math.Clamp(p.Y / PixelsPerMinute, 0, 1440);
        if (current >= _createAnchorMinutes)
        {
            _createStartMinutes = _createAnchorMinutes;
            _createEndMinutes = Math.Max(_createAnchorMinutes + snap, Math.Ceiling(current / snap) * snap);
        }
        else
        {
            _createStartMinutes = Math.Floor(current / snap) * snap;
            _createEndMinutes = _createAnchorMinutes + snap;
        }
        _createEndMinutes = Math.Min(1440, _createEndMinutes);
        UpdateCreatePreview();
    }

    private void AutoScrollWhileCreating()
    {
        if (!_creating) { _autoScroll.Stop(); return; }
        const double edge = 28;
        var y = Mouse.GetPosition(TimeScroll).Y;
        double delta = 0;
        if (y < edge) delta = -Math.Min(24, (edge - y) / 2 + 4);
        else if (y > TimeScroll.ActualHeight - edge) delta = Math.Min(24, (y - (TimeScroll.ActualHeight - edge)) / 2 + 4);
        if (delta == 0) return;
        TimeScroll.ScrollToVerticalOffset(Math.Clamp(TimeScroll.VerticalOffset + delta, 0, TimeScroll.ScrollableHeight));
        UpdateCreateRange(Mouse.GetPosition(TaskCanvas));
    }

    private void UpdateCreatePreview()
    {
        if (_vm is null || _vm.VisibleDays.Count == 0) return;
        var dayWidth = Math.Max(1, TaskCanvas.ActualWidth / _vm.VisibleDays.Count);

        if (_createPreview is null)
        {
            _createPreviewTime = new TextBlock { Foreground = Brushes.White, FontSize = 11, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap };
            var title = new TextBlock { Text = "(Без названия)", Foreground = MetaBrush, FontSize = 10.5, TextTrimming = TextTrimming.CharacterEllipsis };
            var panel = new StackPanel();
            panel.Children.Add(_createPreviewTime);
            panel.Children.Add(title);
            _createPreview = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(225, 26, 115, 232)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(21, 88, 176)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(6, 3, 6, 3),
                ClipToBounds = true,
                Child = panel,
                Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 8, ShadowDepth = 2, Opacity = 0.35 }
            };
            OverlayCanvas.Children.Add(_createPreview);
        }

        _createPreview.Width = Math.Max(30, dayWidth - 6);
        _createPreview.Height = Math.Max(16, (_createEndMinutes - _createStartMinutes) * PixelsPerMinute - 1);
        Canvas.SetLeft(_createPreview, _createDayIndex * dayWidth + 3);
        Canvas.SetTop(_createPreview, _createStartMinutes * PixelsPerMinute);

        var start = TimeSpan.FromMinutes(_createStartMinutes);
        var end = TimeSpan.FromMinutes(_createEndMinutes);
        _createPreviewTime!.Text = $"{FormatClock(start)} – {FormatClock(end)} · {FormatDuration(_createEndMinutes - _createStartMinutes)}";
    }

    private static string FormatClock(TimeSpan t) => t.TotalMinutes >= 1440 ? "24:00" : $"{(int)t.TotalHours:00}:{t.Minutes:00}";

    private void CancelCreate()
    {
        var wasCreating = _creating;
        _creating = false;
        _autoScroll.Stop();
        if (wasCreating && TaskCanvas.IsMouseCaptured) TaskCanvas.ReleaseMouseCapture();
        RemoveCreatePreview();
    }

    private void RemoveCreatePreview()
    {
        if (_createPreview is not null) OverlayCanvas.Children.Remove(_createPreview);
        _createPreview = null;
        _createPreviewTime = null;
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
