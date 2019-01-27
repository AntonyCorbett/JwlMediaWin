namespace JwlMediaWin.Models
{
    internal class Options
    {
        public Options()
        {
            FixerEnabled = true;
            MediaWindowOnTop = true;
        }

        public bool FixerEnabled { get; set; }

        public bool MediaWindowOnTop { get; set; }
    }
}
