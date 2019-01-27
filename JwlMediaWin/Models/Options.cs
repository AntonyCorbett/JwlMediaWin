namespace JwlMediaWin.Models
{
    internal class Options
    {
        public Options()
        {
            FixerEnabledJwLib = true;
            FixerEnabledJwLibSign = false;
            MediaWindowOnTop = true;
        }

        public bool FixerEnabledJwLib { get; set; }

        public bool FixerEnabledJwLibSign { get; set; }

        public bool MediaWindowOnTop { get; set; }
    }
}
