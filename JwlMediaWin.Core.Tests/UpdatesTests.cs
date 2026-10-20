namespace JwlMediaWin.Core.Tests
{
    using System.Security.Cryptography;
    using JwlMediaWin.Core.Updates;

    [TestClass]
    public sealed class UpdatesTests
    {
        [TestMethod]
        public void ParseVersion_FourPartTag_ReturnsVersion()
        {
            var version = UpdateRules.ParseVersion("1.1.0.7");

            Assert.AreEqual(new Version(1, 1, 0, 7), version);
        }

        [TestMethod]
        public void ParseVersion_AcceptsLeadingV()
        {
            Assert.AreEqual(new Version(1, 2, 3, 4), UpdateRules.ParseVersion("v1.2.3.4"));
            Assert.AreEqual(new Version(1, 2, 3, 4), UpdateRules.ParseVersion("V1.2.3.4"));
        }

        [TestMethod]
        public void ParseVersion_RejectsInvalidTags()
        {
            Assert.IsNull(UpdateRules.ParseVersion("1.1.0"));
            Assert.IsNull(UpdateRules.ParseVersion("1.1.0.7.1"));
            Assert.IsNull(UpdateRules.ParseVersion("1.1.0.x"));
            Assert.IsNull(UpdateRules.ParseVersion("1.1.0.-7"));
            Assert.IsNull(UpdateRules.ParseVersion(string.Empty));
            Assert.IsNull(UpdateRules.ParseVersion(null!));
        }

        [TestMethod]
        public void IsNewer_ComparesVersions()
        {
            var release = new ReleaseInfo { Version = new Version(1, 1, 0, 7) };

            Assert.IsTrue(UpdateRules.IsNewer(new Version(1, 1, 0, 6), release));
            Assert.IsFalse(UpdateRules.IsNewer(new Version(1, 1, 0, 7), release));
            Assert.IsFalse(UpdateRules.IsNewer(new Version(1, 2, 0, 0), release));
            Assert.IsFalse(UpdateRules.IsNewer(new Version(1, 1, 0, 6), new ReleaseInfo()));
        }

        [TestMethod]
        public void Verify_SizeMismatch_ReturnsFalse()
        {
            WithTempFile(new byte[] { 1, 2, 3 }, path =>
            {
                Assert.IsFalse(InstallerVerifier.Verify(path, 4, null));
            });
        }

        [TestMethod]
        public void Verify_HashMismatch_ReturnsFalse()
        {
            WithTempFile(new byte[] { 1, 2, 3 }, path =>
            {
                Assert.IsFalse(InstallerVerifier.Verify(path, 3, new string('0', 64)));
            });
        }

        [TestMethod]
        public void Verify_MatchingSizeAndHash_ReturnsTrue()
        {
            var data = new byte[] { 1, 2, 3 };
            var hash = BitConverter.ToString(SHA256.Create().ComputeHash(data)).Replace("-", string.Empty);

            WithTempFile(data, path =>
            {
                Assert.IsTrue(InstallerVerifier.Verify(path, 3, hash.ToLowerInvariant()));
                Assert.IsTrue(InstallerVerifier.Verify(path, 3, hash.ToUpperInvariant()));
            });
        }

        [TestMethod]
        public void Verify_NoHash_ChecksSizeOnly()
        {
            WithTempFile(new byte[] { 1, 2, 3 }, path =>
            {
                Assert.IsTrue(InstallerVerifier.Verify(path, 3, null));
            });
        }

        [TestMethod]
        public void Verify_MissingFile_ReturnsFalse()
        {
            Assert.IsFalse(InstallerVerifier.Verify(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")), 3, null));
        }

        private static void WithTempFile(byte[] data, Action<string> action)
        {
            var path = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(path, data);
                action(path);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
