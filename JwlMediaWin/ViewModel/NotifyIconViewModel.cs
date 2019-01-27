using System.Diagnostics;

namespace JwlMediaWin.ViewModel
{
    using System.Windows;
    using GalaSoft.MvvmLight.CommandWpf;
    using JwlMediaWin.Core;
    using JwlMediaWin.Services;

    internal class NotifyIconViewModel
    {
        private readonly IOptionsService _optionsService;
        private Fixer _fixer;

        public NotifyIconViewModel(IOptionsService optionsService)
        {
            _fixer = new Fixer();
            _optionsService = optionsService;

            ExitAppCommand = new RelayCommand(Application.Current.Shutdown);
            ShowHelpPageCommand = new RelayCommand(ShowHelpPage);
        }

        public RelayCommand ExitAppCommand { get; }

        public RelayCommand ShowHelpPageCommand { get; }

        public bool IsFixEnabled
        {
            get; set; 
        }

        public bool IsKeepOnTopEnabled { get; set; }

        private void ShowHelpPage()
        {
            Process.Start(@"https://github.com/AntonyCorbett/JwlMediaWin/wiki");
        }
    }
}
