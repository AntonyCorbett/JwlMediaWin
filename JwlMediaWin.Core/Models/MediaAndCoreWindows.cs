namespace JwlMediaWin.Core.Models
{
    using System.Windows.Automation;

    internal sealed class MediaAndCoreWindows
    {
        /// <summary>
        /// Gets or sets the JWL media window.
        /// </summary>
        /// <value>
        /// The JWL media window.
        /// </value>
        public AutomationElement MediaWindow { get; set; }

        /// <summary>
        /// Gets or sets the JWL "core" window (a direct child of the media window).
        /// </summary>
        /// <value>
        /// The JWL core window.
        /// </value>
        public AutomationElement CoreWindow { get; set; }
    }
}
