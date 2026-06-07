namespace JwlMediaWin.Core.Tests
{
    using System.Diagnostics;
    using JwlMediaWin.Core.Models;

    [TestClass]
    public sealed class FixerTests
    {
        [TestMethod]
        public void Execute_WhenAppTypeIsNone_ReturnsUnknownErrorStatus()
        {
            var fixer = new Fixer();

            var status = fixer.Execute(JwLibAppTypes.None, topMost: false);

            Assert.IsTrue(status.ErrorUnknown);
            Assert.IsFalse(status.ErrorIsTransitioning);
            Assert.IsFalse(status.IsFixed);
            Assert.IsFalse(status.CoreWindowFocused);
            Assert.IsNull(status.FindWindowResult);
        }

        [TestMethod]
        public void Execute_WhenJwLibraryIsNotRunning_ReturnsStatusIndicatingProcessNotFound()
        {
            AssertProcessNotRunningOrInconclusive("JWLibrary");

            var fixer = new Fixer();

            var status = fixer.Execute(JwLibAppTypes.JwLibrary, topMost: false);

            AssertNotRunningStatus(status);
        }

        [TestMethod]
        public void Execute_WhenJwLibrarySignLanguageIsNotRunning_ReturnsStatusIndicatingProcessNotFound()
        {
            AssertProcessNotRunningOrInconclusive("JWLibrary.Forms.UWP");

            var fixer = new Fixer();

            var status = fixer.Execute(JwLibAppTypes.JwLibrarySignLanguage, topMost: false);

            AssertNotRunningStatus(status);
        }

        private static void AssertProcessNotRunningOrInconclusive(string processName)
        {
            if (Process.GetProcessesByName(processName).Length > 0)
            {
                Assert.Inconclusive($"'{processName}' is running on this machine; cannot verify the 'not running' code path.");
            }
        }

        private static void AssertNotRunningStatus(FixerStatus status)
        {
            Assert.IsNotNull(status.FindWindowResult);
            Assert.IsFalse(status.FindWindowResult.JwlRunning);
            Assert.IsFalse(status.FindWindowResult.FoundMediaWindow);
            Assert.IsFalse(status.FindWindowResult.IsAlreadyFixed);
            Assert.IsFalse(status.IsFixed);
            Assert.IsFalse(status.CoreWindowFocused);
            Assert.IsFalse(status.ErrorUnknown);
            Assert.IsFalse(status.ErrorIsTransitioning);
        }
    }
}
