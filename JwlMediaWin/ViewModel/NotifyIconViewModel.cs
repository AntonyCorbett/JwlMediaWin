namespace JwlMediaWin.ViewModel
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Core;
    using Core.Models;
    using Core.Updates;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using GalaSoft.MvvmLight.Messaging;
    using Hardcodet.Wpf.TaskbarNotification;
    using PubSubMessages;
    using Serilog;
    using Services;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class NotifyIconViewModel : ViewModelBase
    {
        private const string AppTitle = "JwlMediaWin";
        private static readonly TimeSpan InitialUpdateCheckDelay = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan UpdateCheckInterval = TimeSpan.FromHours(24);

        private readonly IOptionsService _optionsService;
        private readonly IUpdateService _updateService;
        private readonly FixerRunner _fixerRunner = new FixerRunner();
        private readonly StatusMessageGenerator _statusMessageGenerator = new StatusMessageGenerator();

        private bool _isFixed;
        private DateTime _restartTipLastShow = DateTime.MinValue;
        private ReleaseInfo _availableRelease;
        private Version _notifiedVersion;
        private CancellationTokenSource _updateCheckCts;
        private bool _updateBusy;

        public NotifyIconViewModel(IOptionsService optionsService, IUpdateService updateService)
        {
            _optionsService = optionsService;
            _updateService = updateService;

            ExitAppCommand = new RelayCommand(Application.Current.Shutdown);
            ShowHelpPageCommand = new RelayCommand(ShowHelpPage);

            ResetFixer();

            _fixerRunner.StatusEvent += HandleFixerRunnerStatusEvent;

            Task.Run(() => { _fixerRunner.Start(); });

            InstallUpdateCommand = new RelayCommand(async () => await InstallUpdateAsync(), CanInstallUpdate);
            CheckForUpdatesNowCommand = new RelayCommand(async () => await CheckForUpdateAsync(true), () => !_updateBusy);

            if (_optionsService.Options.CheckForUpdates)
            {
                StartUpdateChecks();
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public RelayCommand ExitAppCommand { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public RelayCommand ShowHelpPageCommand { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        public RelayCommand InstallUpdateCommand { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        public RelayCommand CheckForUpdatesNowCommand { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsAutoUpdateCheckEnabled
        {
            get => _optionsService.Options.CheckForUpdates;
            set
            {
                if (_optionsService.Options.CheckForUpdates != value)
                {
                    _optionsService.Options.CheckForUpdates = value;
                    _optionsService.Save();
                    RaisePropertyChanged();

                    Log.Logger.Information(value
                        ? "Enabled automatic update check"
                        : "Disabled automatic update check");

                    if (value)
                    {
                        StartUpdateChecks();
                    }
                    else
                    {
                        StopUpdateChecks();
                    }
                }
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsUpdateAvailable => _availableRelease != null;

        // ReSharper disable once MemberCanBePrivate.Global
        public string UpdateMenuHeader => _availableRelease == null
            ? "Install Update"
            : $"Install Update (version {_availableRelease.Version})";

        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsFixEnabledJwLib
        {
            get => _optionsService.Options.FixerEnabledJwLib;
            set
            {
                if (_optionsService.Options.FixerEnabledJwLib != value)
                {
                    _optionsService.Options.FixerEnabledJwLib = value;
                    RaisePropertyChanged();

                    Log.Logger.Information(value
                        ? "Enabled fix for JW Library"
                        : "Disabled fix for JW Library");

                    if (value)
                    {
                        IsFixEnabledJwLibSign = false;
                    }
                    
                    ResetFixer();
                }
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsFixEnabledJwLibSign
        {
            get => _optionsService.Options.FixerEnabledJwLibSign;
            set
            {
                if (_optionsService.Options.FixerEnabledJwLibSign != value)
                {
                    _optionsService.Options.FixerEnabledJwLibSign = value;
                    RaisePropertyChanged();

                    Log.Logger.Information(value
                        ? "Enabled fix for JW Library Sign Language"
                        : "Disabled fix for JW Library Sign Language");

                    if (value)
                    {
                        IsFixEnabledJwLib = false;
                    }

                    ResetFixer();
                }
            }
        }

        public string VersionStringAndCopyright => "version " +
                                       VersionDetection.GetCurrentVersionString() +
                                       ". JW Library is a trademark of Watch Tower Bible and Tract Society of Pennsylvania";

        // ReSharper disable once UnusedMember.Global
        public bool IsKeepOnTopEnabled
        {
            get => _optionsService.Options.MediaWindowOnTop;
            set
            {
                if (_optionsService.Options.MediaWindowOnTop != value)
                {
                    _optionsService.Options.MediaWindowOnTop = value;
                    RaisePropertyChanged();

                    Log.Logger.Information(value
                        ? "Enabled keep on top"
                        : "Disabled on top");
                    
                    ResetFixer();
                }
            }
        }

        private static string GetAppName(JwLibAppTypes appType)
        {
            switch (appType)
            {
                case JwLibAppTypes.JwLibrary:
                    return "JW Library";

                case JwLibAppTypes.JwLibrarySignLanguage:
                    return "JW Library Sign Language";

                default:
                    return "Unknown";
            }
        }

        private void ShowHelpPage()
        {
            Process.Start(@"https://github.com/AntonyCorbett/JwlMediaWin/wiki");
        }

        private void ResetFixer()
        {
            _optionsService.Save();

            if (_optionsService.Options.FixerEnabledJwLib)
            {
                _fixerRunner.AppType = JwLibAppTypes.JwLibrary;
            }
            else if (_optionsService.Options.FixerEnabledJwLibSign)
            {
                _fixerRunner.AppType = JwLibAppTypes.JwLibrarySignLanguage;
            }
            else
            {
                _fixerRunner.AppType = JwLibAppTypes.None;
            }

            _fixerRunner.KeepOnTop = _optionsService.Options.MediaWindowOnTop;

            if (_isFixed && ShouldShowRestart())
            {
                _restartTipLastShow = DateTime.UtcNow;
                ShowBalloonMsg(BalloonIcon.Warning, "Restart JW Library for changes to take effect.");
            }
        }

        private bool ShouldShowRestart()
        {
            return (DateTime.UtcNow - _restartTipLastShow).TotalMinutes > 1;
        }

        private void HandleFixerRunnerStatusEvent(object sender, FixerStatusEventArgs e)
        {
            _isFixed = e.Status.IsFixed || (e.Status.FindWindowResult != null && e.Status.FindWindowResult.IsAlreadyFixed);

            if (e.Status.ErrorUnknown && e.Status.UnknownException != null)
            {
                Log.Logger.Error(e.Status.UnknownException, "Unknown error in fixer");
            }

            var appName = GetAppName(_fixerRunner.AppType);
            var msg = _statusMessageGenerator.Generate(e.Status, appName);

            if (msg != null)
            {
                Log.Logger.Information("Status message: {Message}", msg);

                if (e.Status.IsFixed)
                {
                    ShowBalloonMsg(BalloonIcon.Info, msg);
                }
            }
        }

        private bool CanInstallUpdate()
        {
            return !_updateBusy && _availableRelease != null;
        }

        private void RunOnUiThread(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.Invoke(action);
            }
        }

        private void StartUpdateChecks()
        {
            StopUpdateChecks();

            var cts = new CancellationTokenSource();
            _updateCheckCts = cts;
            Task.Run(() => RunUpdateChecksAsync(cts.Token));
        }

        private void StopUpdateChecks()
        {
            var cts = _updateCheckCts;
            _updateCheckCts = null;

            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
        }

        private async Task RunUpdateChecksAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(InitialUpdateCheckDelay, token).ConfigureAwait(false);

                while (!token.IsCancellationRequested)
                {
                    await CheckForUpdateAsync(false).ConfigureAwait(false);
                    await Task.Delay(UpdateCheckInterval, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // checking was disabled or the app is closing
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Update check loop");
            }
        }

        private async Task CheckForUpdateAsync(bool manual)
        {
            try
            {
                var release = await _updateService.GetNewerReleaseAsync().ConfigureAwait(false);

                if (release == null)
                {
                    if (manual)
                    {
                        RunOnUiThread(() => ShowBalloonMsg(BalloonIcon.Info, AppTitle, "You are running the latest version."));
                    }

                    return;
                }

                RunOnUiThread(() =>
                {
                    _availableRelease = release;
                    RaiseUpdateProperties();

                    if (_notifiedVersion != release.Version)
                    {
                        _notifiedVersion = release.Version;
                        ShowBalloonMsg(
                            BalloonIcon.Info,
                            AppTitle,
                            $"Version {release.Version} is available. Right-click the tray icon to install it.");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Checking for updates");
            }
        }

        private void RaiseUpdateProperties()
        {
            RaisePropertyChanged(nameof(IsUpdateAvailable));
            RaisePropertyChanged(nameof(UpdateMenuHeader));
            InstallUpdateCommand.RaiseCanExecuteChanged();
            CheckForUpdatesNowCommand.RaiseCanExecuteChanged();
        }

        private void SetUpdateBusy(bool busy)
        {
            RunOnUiThread(() =>
            {
                _updateBusy = busy;
                RaiseUpdateProperties();
            });
        }

        private async Task InstallUpdateAsync()
        {
            var release = _availableRelease;
            if (release == null || _updateBusy)
            {
                return;
            }

            SetUpdateBusy(true);

            try
            {
                var path = await _updateService.DownloadInstallerAsync(release).ConfigureAwait(false);
                _updateService.LaunchInstaller(path);

                RunOnUiThread(() => Application.Current.Shutdown());
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Installing update");

                SetUpdateBusy(false);

                RunOnUiThread(() => ShowBalloonMsg(
                    BalloonIcon.Error,
                    AppTitle,
                    "The update could not be installed. Opening the release page instead."));

                try
                {
                    if (!string.IsNullOrEmpty(release.ReleasePageUrl))
                    {
                        Process.Start(release.ReleasePageUrl);
                    }
                }
                catch (Exception browserEx)
                {
                    Log.Logger.Error(browserEx, "Opening release page");
                }
            }
        }

        private void ShowBalloonMsg(BalloonIcon icon, string title, string msg)
        {
            Messenger.Default.Send(new ShowBalloonTipMessage
            {
                IconType = icon,
                Title = title,
                Message = msg
            });
        }

        private void ShowBalloonMsg(BalloonIcon icon, string msg)
        {
            Messenger.Default.Send(new ShowBalloonTipMessage
            {
                IconType = icon,
                Title = $"{GetAppName(_fixerRunner.AppType)}",
                Message = msg
            });
        }
    }
}
