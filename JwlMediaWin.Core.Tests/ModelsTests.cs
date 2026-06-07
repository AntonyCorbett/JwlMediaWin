namespace JwlMediaWin.Core.Tests
{
    using JwlMediaWin.Core.Models;

    [TestClass]
    public sealed class ModelsTests
    {
        [TestMethod]
        public void FixerStatusEventArgs_ExposesGivenStatus()
        {
            var status = new FixerStatus { IsFixed = true };

            var args = new FixerStatusEventArgs { Status = status };

            Assert.AreSame(status, args.Status);
        }

        [TestMethod]
        public void FixerStatus_DefaultsToNoErrors()
        {
            var status = new FixerStatus();

            Assert.IsFalse(status.CoreWindowFocused);
            Assert.IsFalse(status.IsFixed);
            Assert.IsFalse(status.ErrorIsTransitioning);
            Assert.IsFalse(status.ErrorUnknown);
            Assert.IsNull(status.FindWindowResult);
        }

        [TestMethod]
        public void FindWindowResult_DefaultsToNotFound()
        {
            var result = new FindWindowResult();

            Assert.IsFalse(result.JwlRunning);
            Assert.IsFalse(result.FoundMediaWindow);
            Assert.IsFalse(result.IsAlreadyFixed);
        }
    }
}
