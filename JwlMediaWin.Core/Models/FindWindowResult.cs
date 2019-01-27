namespace JwlMediaWin.Core.Models
{
    using System.Windows.Automation;

    public class FindWindowResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether JWL is running.
        /// </summary>
        /// <value>
        ///   <c>true</c> if JWL is running; otherwise, <c>false</c>.
        /// </value>
        public bool JwlRunning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the desktop element has been found.
        /// </summary>
        /// <value>
        ///   <c>true</c> if found; otherwise, <c>false</c>.
        /// </value>
        public bool FoundDesktop { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the JWL media window has been found.
        /// </summary>
        /// <value>
        ///   <c>true</c> if found; otherwise, <c>false</c>.
        /// </value>
        public bool FoundMediaWindow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the media window is fixed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is fixed; otherwise, <c>false</c>.
        /// </value>
        public bool IsAlreadyFixed { get; set; }

        /// <summary>
        /// Gets or sets the main media window element.
        /// </summary>
        /// <value>
        /// The main media window.
        /// </value>
        public AutomationElement MainMediaWindow { get; set; }

        /// <summary>
        /// Gets or sets the 'core' media window.
        /// </summary>
        /// <value>
        /// The core media window.
        /// </value>
        public AutomationElement CoreMediaWindow { get; set; }
    }
}
