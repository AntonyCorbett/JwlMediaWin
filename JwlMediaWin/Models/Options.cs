namespace JwlMediaWin.Models
{
    internal sealed class Options
    {
        public Options()
        {
            FixerEnabledJwLib = true;
            FixerEnabledJwLibSign = false;
            MediaWindowOnTop = true;
            CheckForUpdates = true;
        }

        public bool FixerEnabledJwLib { get; set; }

        public bool FixerEnabledJwLibSign { get; set; }

        public bool MediaWindowOnTop { get; set; }

        public bool CheckForUpdates { get; set; }
    }
}
