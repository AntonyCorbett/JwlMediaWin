namespace JwlMediaWin
{
    using System.IO;
    using System.Threading;
    using System.Windows;
    using GalaSoft.MvvmLight.Messaging;
    using Hardcodet.Wpf.TaskbarNotification;
    using PubSubMessages;
    using Serilog;
    using ViewModel;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly string _appString = "JwlMediaWindowFixSoundBox";
        private Mutex _appMutex;
        private TaskbarIcon _notifyIcon;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (AnotherInstanceRunning())
            {
                Shutdown();
            }
            else
            {
                ConfigureLogger();
                ViewModelLocator.Init();
                _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

                if (_notifyIcon != null)
                {
                    _notifyIcon.DataContext = ViewModelLocator.MainViewModel;
                    Messenger.Default.Register<ShowBalloonTipMessage>(this, OnShowBalloonTipMessage);
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _notifyIcon?.Dispose();
            _appMutex?.Dispose();

            Log.Logger.Information("==== Exit ====");
            base.OnExit(e);
        }

        private static void ConfigureLogger()
        {
            string logsDirectory = FileUtils.GetLogFolder();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.RollingFile(Path.Combine(logsDirectory, "log-{Date}.txt"), retainedFileCountLimit: 28)
#if DEBUG
                .WriteTo.Console()
#endif
                .CreateLogger();

            Log.Logger.Information("==== Launched ====");
            Log.Logger.Information("Version {Version}", VersionDetection.GetCurrentVersion());
        }

        private bool AnotherInstanceRunning()
        {
            _appMutex = new Mutex(true, _appString, out var newInstance);
            return !newInstance;
        }

        private void OnShowBalloonTipMessage(ShowBalloonTipMessage message)
        {
            _notifyIcon.ShowBalloonTip(message.Title, message.Message, message.IconType);
        }
    }
}
