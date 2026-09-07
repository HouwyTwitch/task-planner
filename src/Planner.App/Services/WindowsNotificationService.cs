using System.IO;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace Planner.App.Services;

/// <summary>
/// Штатные уведомления Windows 10/11 через Windows App SDK.
/// Собственных WPF popup-окон для уведомлений приложение не создаёт.
/// </summary>
public sealed class WindowsNotificationService : INotificationService, IDisposable
{
    private readonly Action _onActivated;
    private bool _registered;

    public WindowsNotificationService(Action onActivated)
    {
        _onActivated=onActivated;
    }

    public bool IsSupported
    {
        get
        {
            try{return AppNotificationManager.IsSupported();}
            catch{return false;}
        }
    }

    public void Register()
    {
        if(_registered || !IsSupported)return;
        try
        {
            AppNotificationManager.Default.NotificationInvoked+=OnNotificationInvoked;

            var iconPath=Path.Combine(AppContext.BaseDirectory,"Assets","NetworkPlanner.png");
            if(File.Exists(iconPath))
                AppNotificationManager.Default.Register("Сетевой планировщик",new Uri(iconPath,UriKind.Absolute));
            else
                AppNotificationManager.Default.Register();

            _registered=true;
        }
        catch
        {
            AppNotificationManager.Default.NotificationInvoked-=OnNotificationInvoked;
            _registered=false;
        }
    }

    public void Show(string title,string message)
    {
        if(!_registered)return;
        try
        {
            var builder=new AppNotificationBuilder()
                .AddArgument("action","openPlanner")
                .AddText(string.IsNullOrWhiteSpace(title)?"Сетевой планировщик":title);

            foreach(var line in SplitMessage(message))
                builder.AddText(line);

            AppNotificationManager.Default.Show(builder.BuildNotification());
        }
        catch
        {
            // Ошибка системного центра уведомлений не должна останавливать планировщик.
        }
    }

    private static IEnumerable<string> SplitMessage(string message)
    {
        if(string.IsNullOrWhiteSpace(message))return ["Откройте Сетевой планировщик для просмотра."];
        return message.Replace("\r",string.Empty).Split('\n',StringSplitOptions.RemoveEmptyEntries).Take(2);
    }

    private void OnNotificationInvoked(AppNotificationManager sender,AppNotificationActivatedEventArgs args)
    {
        _onActivated();
    }

    public void Dispose()
    {
        if(!_registered)return;
        try
        {
            AppNotificationManager.Default.NotificationInvoked-=OnNotificationInvoked;
            AppNotificationManager.Default.Unregister();
        }
        catch
        {
        }
        finally
        {
            _registered=false;
        }
    }
}
