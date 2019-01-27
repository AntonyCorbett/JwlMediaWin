using System.Net.NetworkInformation;

namespace JwlMediaWin.ViewModel
{
    using CommonServiceLocator;
    using GalaSoft.MvvmLight.Ioc;
    using JwlMediaWin.Services;

    internal class ViewModelLocator
    {
        public static void Init()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IOptionsService, OptionsService>();

            SimpleIoc.Default.Register<NotifyIconViewModel>();
        }

        public static NotifyIconViewModel MainViewModel => ServiceLocator.Current.GetInstance<NotifyIconViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}