using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Windows.AppNotifications;
using Planner.App.Services;
using Planner.App.ViewModels;
using Planner.Data.Configuration;
using Planner.Data.Database;
using Planner.Data.Repositories;
using Planner.Data.Services;

namespace Planner.App;

public partial class App : Application
{
    private CancellationTokenSource? _cts;
    private SignalWatcher? _signalWatcher;
    private BackgroundCoordinator? _background;
    private WindowsNotificationService? _notifications;

    protected override async void OnStartup(StartupEventArgs e)
    {
        ConfigureRussianCulture();
        base.OnStartup(e);
        _cts=new CancellationTokenSource();
        try
        {
            _notifications=new WindowsNotificationService(ActivateMainWindow);
            _notifications.Register();

            var settingsPath=Path.Combine(AppContext.BaseDirectory,"planner.settings.json");
            var settings=PlannerSettings.Load(settingsPath);
            settings.Validate();
            if(settings.SharedFolder.Contains(@"\\server\share",StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(@"Укажите реальный UNC-путь SharedFolder в planner.settings.json, например \\fileserver\Planner$\Planner.");

            var netLock=new NetworkDatabaseLock(settings);
            var factory=new SqliteConnectionFactory(settings);
            var db=new DatabaseExecutor(netLock,factory);
            await new DatabaseInitializer(settings,db).InitializeAsync(_cts.Token);

            var userRepo=new UserRepository(db);
            var taskRepo=new TaskRepository(db);
            var changeRepo=new ChangeLogRepository(db);
            var reminderRepo=new ReminderRepository(db);
            var signalService=new SignalService(settings);
            var taskService=new TaskService(userRepo,taskRepo,signalService);
            var adminService=new UserAdminService(userRepo);
            var recurrence=new RecurringTaskProcessor(db,settings,signalService);

            var username=Environment.UserName;
            var logged=await userRepo.GetByUsernameAsync(username,_cts.Token);
            if(logged is null)
            {
                var all=await userRepo.GetAllAsync(_cts.Token);
                if(all.Count==0) logged=await userRepo.BootstrapFirstUserAsync(username,username,_cts.Token);
                else throw new UnauthorizedAccessException($"Пользователь Windows '{username}' не зарегистрирован. Администратор должен добавить его в «Сетевой планировщик» с такой же учётной записью Windows.");
            }

            // Сразу формируем будущие экземпляры повторяющихся задач до первой отрисовки календаря.
            await recurrence.ProcessAsync(_cts.Token);

            var dialogs=new DialogService();
            var vm=new MainViewModel(settings,logged,taskService,adminService,changeRepo,recurrence,dialogs,_notifications);
            await vm.InitializeAsync(_cts.Token);

            var window=new MainWindow{DataContext=vm};
            MainWindow=window;
            window.Show();

            _signalWatcher=new SignalWatcher(settings,logged.Id);
            _signalWatcher.SignalReceived+=async (_,_)=> await Dispatcher.InvokeAsync(()=>vm.HandleExternalSignalAsync(_cts.Token)).Task.Unwrap();

            _background=new BackgroundCoordinator(settings,logged.Id,recurrence,reminderRepo,vm,_notifications);
            _background.Start(_cts.Token);
        }
        catch(Exception ex)
        {
            MessageBox.Show(ex.Message,"Сетевой планировщик — ошибка запуска",MessageBoxButton.OK,MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static void ConfigureRussianCulture()
    {
        var ru=CultureInfo.GetCultureInfo("ru-RU");
        CultureInfo.DefaultThreadCurrentCulture=ru;
        CultureInfo.DefaultThreadCurrentUICulture=ru;
        CultureInfo.CurrentCulture=ru;
        CultureInfo.CurrentUICulture=ru;

        // WPF Binding/StringFormat использует Language элемента, а не только культуру потока.
        // Это гарантирует русские названия дней и месяцев во всех представлениях календаря.
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("ru-RU")));
    }

    private void ActivateMainWindow()
    {
        Dispatcher.Invoke(() =>
        {
            if(MainWindow is null)return;
            if(MainWindow.WindowState==WindowState.Minimized)MainWindow.WindowState=WindowState.Normal;
            MainWindow.Show();
            MainWindow.Activate();
            MainWindow.Topmost=true;
            MainWindow.Topmost=false;
            MainWindow.Focus();
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _cts?.Cancel();
        _signalWatcher?.Dispose();
        _background?.Dispose();
        _notifications?.Dispose();
        _cts?.Dispose();
        base.OnExit(e);
    }
}
