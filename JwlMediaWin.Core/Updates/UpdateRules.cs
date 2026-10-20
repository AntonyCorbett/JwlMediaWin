namespace JwlMediaWin.Core.Updates
{
    using System;
    using System.Globalization;

    public static class UpdateRules
    {
        public static Version ParseVersion(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return null;
            }

            var text = tag.Trim();
            if (text.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring(1);
            }

            var tokens = text.Split('.');
            if (tokens.Length != 4)
            {
                return null;
            }

            var parts = new int[4];
            for (var n = 0; n < 4; ++n)
            {
                if (!int.TryParse(tokens[n], NumberStyles.None, CultureInfo.InvariantCulture, out parts[n]))
                {
                    return null;
                }
            }

            return new Version(parts[0], parts[1], parts[2], parts[3]);
        }

        public static bool IsNewer(Version current, ReleaseInfo release)
        {
            if (current == null || release == null || release.Version == null)
            {
                return false;
            }

            return release.Version > current;
        }
    }
}
