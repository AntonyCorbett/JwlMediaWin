namespace JwlMediaWin.Core.Tests
{
    [TestClass]
    public sealed class NativeMethodsTests
    {
        [TestMethod]
        public void Width_ReturnsDifferenceBetweenRightAndLeft()
        {
            var rect = new NativeMethods.RECT { Left = 10, Top = 20, Right = 110, Bottom = 220 };

            Assert.AreEqual(100, rect.Width);
        }

        [TestMethod]
        public void Height_ReturnsDifferenceBetweenBottomAndTop()
        {
            var rect = new NativeMethods.RECT { Left = 10, Top = 20, Right = 110, Bottom = 220 };

            Assert.AreEqual(200, rect.Height);
        }

        [TestMethod]
        public void Width_WhenLeftAndRightAreEqual_ReturnsZero()
        {
            var rect = new NativeMethods.RECT { Left = 50, Top = 0, Right = 50, Bottom = 0 };

            Assert.AreEqual(0, rect.Width);
        }

        [TestMethod]
        public void Height_WhenTopAndBottomAreEqual_ReturnsZero()
        {
            var rect = new NativeMethods.RECT { Left = 0, Top = 50, Right = 0, Bottom = 50 };

            Assert.AreEqual(0, rect.Height);
        }
    }
}
