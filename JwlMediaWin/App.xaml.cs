namespace JwlMediaWin
{
    using System.IO;
    using System.Threading;
    using System.Windows;
    using Hardcodet.Wpf.TaskbarNotification;
    using JwlMediaWin.ViewModel;
    using Serilog;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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

        private void ConfigureLogger()
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
            Log.Logger.Information($"Version {VersionDetection.GetCurrentVersion()}");
        }

        private bool AnotherInstanceRunning()
        {
            _appMutex = new Mutex(true, _appString, out var newInstance);
            return !newInstance;
        }
    }
}
