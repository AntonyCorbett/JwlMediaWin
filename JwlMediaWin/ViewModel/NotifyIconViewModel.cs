namespace JwlMediaWin.ViewModel
{
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
        }

        private string GetAppName(JwLibAppTypes appType)
        {
            if (appType == JwLibAppTypes.JwLibrary)
            {
                return "JW Library";
            }

            if (appType == JwLibAppTypes.JwLibrarySignLanguage)
            {
                return "JW Library Sign Language";
            }

            return "Unknown";
        }

        private void HandleFixerRunnerStatusEvent(object sender, FixerStatusEventArgs e)
        {
            var appName = GetAppName(_fixerRunner.AppType);
            var msg = _statusMessageGenerator.Generate(e.Status, appName);

            if (msg != null)
            {
                Log.Logger.Information(msg);

                if (e.Status.IsFixed)
                {
                    Messenger.Default.Send(new ShowBalloonTipMessage
                    {
                        IconType = BalloonIcon.Info,
                        Title = $"{appName}",
                        Message = msg
                    });
                }
            }
        }
    }
}
