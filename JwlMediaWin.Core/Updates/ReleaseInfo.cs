namespace JwlMediaWin.Core.Updates
{
    using System;

    public sealed class ReleaseInfo
    {
        public Version Version { get; set; }

        public string TagName { get; set; }

        public string ReleasePageUrl { get; set; }

        public string InstallerUrl { get; set; }

        public long InstallerSize { get; set; }

        public string InstallerSha256 { get; set; }
    }
}
