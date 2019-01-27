namespace JwlMediaWin.ViewModel
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using JwlMediaWin.Core;
    using JwlMediaWin.Core.Models;
    using JwlMediaWin.Services;

    internal class NotifyIconViewModel : ViewModelBase
    {
        private readonly IOptionsService _optionsService;
        private readonly FixerRunner _fixerRunner = new FixerRunner();

        public NotifyIconViewModel(IOptionsService optionsService)
        {
            _optionsService = optionsService;

            ExitAppCommand = new RelayCommand(Application.Current.Shutdown);
            ShowHelpPageCommand = new RelayCommand(ShowHelpPage);
            
            ResetFixer();

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

                    if (value)
                    {
                        IsFixEnabledJwLibSign = false;
                    }

                    _optionsService.Save();
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

                    if (value)
                    {
                        IsFixEnabledJwLib = false;
                    }
                    
                    _optionsService.Save();
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

                    _optionsService.Save();
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
    }
}
