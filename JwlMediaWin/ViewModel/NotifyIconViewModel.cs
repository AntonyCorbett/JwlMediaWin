namespace JwlMediaWin.ViewModel
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using GalaSoft.MvvmLight.Messaging;
    using Hardcodet.Wpf.TaskbarNotification;
    using JwlMediaWin.Core;
    using JwlMediaWin.Core.Models;
    using JwlMediaWin.PubSubMessages;
    using JwlMediaWin.Services;
    using Serilog;

    internal class NotifyIconViewModel : ViewModelBase
    {
        private readonly IOptionsService _optionsService;
        private readonly FixerRunner _fixerRunner = new FixerRunner();
        private readonly StatusMessageGenerator _statusMessageGenerator = new StatusMessageGenerator();

        private bool _isFixed;
        private DateTime _restartTipLastShow = DateTime.MinValue;

        public NotifyIconViewModel(IOptionsService optionsService)
        {
            _optionsService = optionsService;

            ExitAppCommand = new RelayCommand(Application.Current.Shutdown);
            ShowHelpPageCommand = new RelayCommand(ShowHelpPage);
            
            ResetFixer();

            _fixerRunner.StatusEvent += HandleFixerRunnerStatusEvent;

            Task.Run(() => { _fixerRunner.Start(); });
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public RelayCommand ExitAppCommand { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public RelayCommand ShowHelpPageCommand { get; }

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
                        ? "Enabled fix for JW Lib"
                        : "Disabled fix for JW Lib");

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
                        ? "Enabled fix for JW Lib Sign Language"
                        : "Disabled fix for JW Lib Sign Language");

                    if (value)
                    {
                        IsFixEnabledJwLib = false;
                    }
                    
                    ResetFixer();
                }
            }
        }

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

        private string GetAppName(JwLibAppTypes appType)
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

        private void HandleFixerRunnerStatusEvent(object sender, FixerStatusEventArgs e)
        {
            _isFixed = e.Status.IsFixed || (e.Status.FindWindowResult != null && e.Status.FindWindowResult.IsAlreadyFixed);

            var appName = GetAppName(_fixerRunner.AppType);
            var msg = _statusMessageGenerator.Generate(e.Status, appName);

            if (msg != null)
            {
                Log.Logger.Information(msg);

                if (e.Status.IsFixed)
                {
                    ShowBalloonMsg(BalloonIcon.Info, msg);
                }
            }
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
