namespace JwlMediaWin.Services
{
    using System.Threading.Tasks;
    using Core.Updates;

#pragma warning disable U2U1005
    internal interface IUpdateService
#pragma warning restore U2U1005
    {
        Task<ReleaseInfo> GetNewerReleaseAsync();

        Task<string> DownloadInstallerAsync(ReleaseInfo release);

        void LaunchInstaller(string path);
    }
}
