namespace JwlMediaWin.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Updates;
    using Serilog;

    internal sealed class UpdateService : IUpdateService
    {
        private const string LatestReleaseApiUrl = "https://api.github.com/repos/AntonyCorbett/JwlMediaWin/releases/latest";
        private const string InstallerAssetName = "JwlMediaWinSetup.exe";
        private const string InstallerArguments = "/SILENT /SP- /NOCANCEL /CLOSEAPPLICATIONS /updater=1";

        private static readonly HttpClient Client = CreateClient();

        public async Task<ReleaseInfo> GetNewerReleaseAsync()
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, LatestReleaseApiUrl))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

                    using (var response = await Client.SendAsync(request).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        var release = GitHubReleaseParser.Parse(json, InstallerAssetName);
                        if (release == null)
                        {
                            Log.Logger.Information("No usable release found");
                            return null;
                        }

                        var current = VersionDetection.GetCurrentVersion();
                        if (!UpdateRules.IsNewer(current, release))
                        {
                            Log.Logger.Information("Latest release {Latest} is not newer than {Current}", release.Version, current);
                            return null;
                        }

                        Log.Logger.Information("Newer release {Latest} found", release.Version);
                        return release;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Checking for updates");
                return null;
            }
        }

        public async Task<string> DownloadInstallerAsync(ReleaseInfo release)
        {
            if (release == null)
            {
                throw new ArgumentNullException(nameof(release));
            }

            var folder = Path.Combine(Path.GetTempPath(), "JwlMediaWin", "Updates", release.Version.ToString());
            Directory.CreateDirectory(folder);

            var finalPath = Path.Combine(folder, InstallerAssetName);
            var partialPath = finalPath + ".partial";

            File.Delete(partialPath);
            File.Delete(finalPath);

            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(15)))
                using (var request = new HttpRequestMessage(HttpMethod.Get, release.InstallerUrl))
                using (var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    using (var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var target = new FileStream(partialPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                    {
                        await source.CopyToAsync(target, 81920, cts.Token).ConfigureAwait(false);
                    }
                }

                File.Move(partialPath, finalPath);

                if (!InstallerVerifier.Verify(finalPath, release.InstallerSize, release.InstallerSha256))
                {
                    throw new InvalidDataException("The downloaded installer failed size or checksum verification.");
                }

                Log.Logger.Information("Downloaded installer to {Path}", finalPath);
                return finalPath;
            }
            catch (Exception)
            {
                File.Delete(partialPath);
                File.Delete(finalPath);
                throw;
            }
        }

        public void LaunchInstaller(string path)
        {
            Log.Logger.Information("Launching installer {Path}", path);

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                Arguments = InstallerArguments,
                UseShellExecute = true
            });
        }

        private static HttpClient CreateClient()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("JwlMediaWin/" + VersionDetection.GetCurrentVersionString());
            return client;
        }
    }
}
