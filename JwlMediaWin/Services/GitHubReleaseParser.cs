namespace JwlMediaWin.Services
{
    using System;
    using System.Linq;
    using Core.Updates;
    using Newtonsoft.Json.Linq;

    internal static class GitHubReleaseParser
    {
        public static ReleaseInfo Parse(string json, string installerAssetName)
        {
            try
            {
                var root = JObject.Parse(json);

                if ((bool?)root["draft"] == true || (bool?)root["prerelease"] == true)
                {
                    return null;
                }

                var tag = (string)root["tag_name"];
                var version = UpdateRules.ParseVersion(tag);
                if (version == null)
                {
                    return null;
                }

                var assets = root["assets"] as JArray;
                if (assets == null)
                {
                    return null;
                }

                var asset = assets
                    .OfType<JObject>()
                    .FirstOrDefault(a => string.Equals((string)a["name"], installerAssetName, StringComparison.OrdinalIgnoreCase));

                if (asset == null)
                {
                    return null;
                }

                var size = (long?)asset["size"] ?? 0;
                if (size <= 0)
                {
                    return null;
                }

                var url = (string)asset["browser_download_url"];
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                {
                    return null;
                }

                return new ReleaseInfo
                {
                    Version = version,
                    TagName = tag,
                    ReleasePageUrl = (string)root["html_url"],
                    InstallerUrl = uri.AbsoluteUri,
                    InstallerSize = size,
                    InstallerSha256 = ParseDigest((string)asset["digest"])
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string ParseDigest(string digest)
        {
            const string Prefix = "sha256:";

            if (string.IsNullOrWhiteSpace(digest) ||
                !digest.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var hex = digest.Substring(Prefix.Length).Trim().ToLowerInvariant();
            return hex.Length == 64 ? hex : null;
        }
    }
}
