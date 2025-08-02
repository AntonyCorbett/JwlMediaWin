namespace JwlMediaWin.ViewModel
{
    using CommonServiceLocator;
    using GalaSoft.MvvmLight.Ioc;
    using Services;

#pragma warning disable U2U1001
    internal sealed class ViewModelLocator
#pragma warning restore U2U1001
    {
        public static NotifyIconViewModel MainViewModel => ServiceLocator.Current.GetInstance<NotifyIconViewModel>();

        public static void Init()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IOptionsService, OptionsService>();

            SimpleIoc.Default.Register<NotifyIconViewModel>();
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}