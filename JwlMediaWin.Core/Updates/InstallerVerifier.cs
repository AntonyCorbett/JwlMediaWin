namespace JwlMediaWin.Core.Updates
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class InstallerVerifier
    {
        public static bool Verify(string filePath, long expectedSize, string expectedSha256)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            if (new FileInfo(filePath).Length != expectedSize)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(expectedSha256))
            {
                return true;
            }

            using (var stream = File.OpenRead(filePath))
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(stream);
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return string.Equals(sb.ToString(), expectedSha256.Trim(), StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
