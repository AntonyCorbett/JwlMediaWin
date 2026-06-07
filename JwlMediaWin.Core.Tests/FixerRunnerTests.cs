namespace JwlMediaWin.Core.Tests
{
    using JwlMediaWin.Core.Models;

    [TestClass]
    public sealed class FixerRunnerTests
    {
        // Mirrors the private interval constants in FixerRunner.
        private const int IntervalMillisecondsWatchingForProcess = 3 * 1000;
        private const int IntervalMillisecondsWatchingForWindow = 6 * 1000;

        [TestMethod]
        public void GetIntervalMilliseconds_WhenFindWindowResultIsNull_ReturnsWatchingForWindowInterval()
        {
            var status = new FixerStatus { FindWindowResult = null! };

            var interval = FixerRunner.GetIntervalMilliseconds(status);

            Assert.AreEqual(IntervalMillisecondsWatchingForWindow, interval);
        }

        [TestMethod]
        public void GetIntervalMilliseconds_WhenJwlIsRunning_ReturnsWatchingForProcessInterval()
        {
            var status = new FixerStatus { FindWindowResult = new FindWindowResult { JwlRunning = true } };

            var interval = FixerRunner.GetIntervalMilliseconds(status);

            Assert.AreEqual(IntervalMillisecondsWatchingForProcess, interval);
        }

        [TestMethod]
        public void GetIntervalMilliseconds_WhenJwlIsNotRunning_ReturnsWatchingForWindowInterval()
        {
            var status = new FixerStatus { FindWindowResult = new FindWindowResult { JwlRunning = false } };

            var interval = FixerRunner.GetIntervalMilliseconds(status);

            Assert.AreEqual(IntervalMillisecondsWatchingForWindow, interval);
        }

        [TestMethod]
        public void AppType_DefaultsToNone()
        {
            var runner = new FixerRunner();

            Assert.AreEqual(JwLibAppTypes.None, runner.AppType);
        }

        [TestMethod]
        public void KeepOnTop_DefaultsToFalse()
        {
            var runner = new FixerRunner();

            Assert.IsFalse(runner.KeepOnTop);
        }

        [TestMethod]
        public void AppType_CanBeSetAndRead()
        {
            var runner = new FixerRunner { AppType = JwLibAppTypes.JwLibrarySignLanguage };

            Assert.AreEqual(JwLibAppTypes.JwLibrarySignLanguage, runner.AppType);
        }

        [TestMethod]
        public void KeepOnTop_CanBeSetAndRead()
        {
            var runner = new FixerRunner { KeepOnTop = true };

            Assert.IsTrue(runner.KeepOnTop);
        }
    }
}
