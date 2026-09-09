using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using Planner.App.Services;
using Planner.App.ViewModels;
using Planner.App.Views;
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
    private MainViewModel? _mainViewModel;

    protected override async void OnStartup(StartupEventArgs e)
    {
        ConfigureRussianCulture();
        ConfigureGlobalErrorHandling();
        base.OnStartup(e);
        _cts=new CancellationTokenSource();
        try
        {
            _notifications=new WindowsNotificationService(ActivateMainWindow);
            _notifications.Register();

            var settingsStore=new SettingsStore();
            var settings=settingsStore.Load();
            if(settings.Check() is string settingsError && !TryFixSettings(settingsStore,settings,settingsError))
            {
                Shutdown(1);
                return;
            }

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
            var vm=new MainViewModel(settings,settingsStore,logged,taskService,adminService,changeRepo,recurrence,dialogs,_notifications);
            _mainViewModel=vm;
            await vm.InitializeAsync(_cts.Token);

            var window=new MainWindow{DataContext=vm};
            MainWindow=window;
            window.Show();

            _signalWatcher=new SignalWatcher(settings,logged.Id);
            _signalWatcher.SignalReceived+=async (_,_)=> await Dispatcher.InvokeAsync(()=>vm.HandleExternalSignalAsync(_cts.Token)).Task.Unwrap();

            _background=new BackgroundCoordinator(settings,logged.Id,recurrence,reminderRepo,vm,_notifications);
            _background.Start(_cts.Token);

            if(settings.CheckUpdatesOnStartup && settings.HasUpdateFolder)
                _=CheckUpdatesInBackgroundAsync(settings,vm,_cts.Token);
        }
        catch(Exception ex)
        {
            LogException("Ошибка запуска",ex);
            MessageBox.Show($"{ex.Message}\n\nПодробности записаны в журнал:\n{LogFilePath}","Сетевой планировщик — ошибка запуска",MessageBoxButton.OK,MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    /// <summary>
    /// Настройки заполнены неверно — например путь к общей базе ещё не указан.
    /// Программа не закрывается молча, а предлагает открыть окно настроек.
    /// </summary>
    private static bool TryFixSettings(SettingsStore store,PlannerSettings settings,string error)
    {
        var answer=MessageBox.Show(
            $"{error}\n\nОткрыть настройки, чтобы указать путь?",
            "Сетевой планировщик — настройка",MessageBoxButton.YesNo,MessageBoxImage.Warning);
        if(answer!=MessageBoxResult.Yes) return false;

        var editor=new SettingsViewModel(store,settings);
        var window=new SettingsWindow{DataContext=editor};
        if(window.ShowDialog()!=true) return false;

        return settings.Check() is null;
    }

    /// <summary>Тихая проверка обновления при запуске: мешать работе она не должна.</summary>
    private async Task CheckUpdatesInBackgroundAsync(PlannerSettings settings,MainViewModel vm,CancellationToken ct)
    {
        try
        {
            var update=await new UpdateService(settings).CheckAsync(TimeSpan.FromSeconds(10),ct);
            if(update is null) return;
            await Dispatcher.InvokeAsync(()=>
                vm.StatusText=$"{UpdateService.DescribeUpdate(update)}. Установить: «Настройки» → «Проверить обновления».");
            _notifications?.Show("Сетевой планировщик",UpdateService.DescribeUpdate(update));
        }
        catch(OperationCanceledException){}
        catch(Exception ex)
        {
            // Недоступная папка обновлений не должна беспокоить пользователя всплывающим окном.
            LogException("Проверка обновления",ex);
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

    /// <summary>
    /// Непредвиденная ошибка не должна закрывать приложение молча: она попадает в журнал,
    /// пользователь получает понятное окно, а работа продолжается.
    /// </summary>
    private void ConfigureGlobalErrorHandling()
    {
        DispatcherUnhandledException+=OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException+=(_,args)=>LogException("Критическая ошибка",args.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException+=(_,args)=>{LogException("Фоновая задача",args.Exception);args.SetObserved();};
    }

    private void OnDispatcherUnhandledException(object sender,DispatcherUnhandledExceptionEventArgs args)
    {
        LogException("Ошибка интерфейса",args.Exception);
        args.Handled=true;
        MessageBox.Show(
            $"{args.Exception.Message}\n\nПодробности записаны в журнал:\n{LogFilePath}",
            "Сетевой планировщик — ошибка",MessageBoxButton.OK,MessageBoxImage.Warning);
    }

    private static string LogFilePath => AppInfo.LogFilePath;

    private static void LogException(string scope,Exception? exception)
    {
        if(exception is null)return;
        try
        {
            var path=LogFilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            // Журнал не должен расти бесконечно на рабочем месте пользователя.
            if(File.Exists(path)&&new FileInfo(path).Length>1_000_000) File.Delete(path);
            File.AppendAllText(path,$"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{scope}] {exception}{Environment.NewLine}{Environment.NewLine}");
        }
        catch
        {
            // Журналирование не должно порождать новую ошибку.
        }
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
        // Масштаб сетки календаря и прочие изменения из главного окна сохраняются при выходе.
        _mainViewModel?.PersistSettings();
        _cts?.Cancel();
        _signalWatcher?.Dispose();
        _background?.Dispose();
        _notifications?.Dispose();
        _cts?.Dispose();
        base.OnExit(e);
    }
}
