namespace JwlMediaWin.Services
{
    using JwlMediaWin.Core.Models;
    
    internal class StatusMessageGenerator
    {
        private string _previousMessage;

        public string Generate(FixerStatus results, string appName)
        {
            string result;

            if (results.ErrorIsTransitioning)
            {
                result = $"{appName} is transitioning.";
            }
            else if (results.ErrorUnknown)
            {
                result = "Unknown error.";
            }
            else if (!results.FindWindowResult.JwlRunning)
            {
                result = $"{appName} is not running.";
            }
            else if (!results.FindWindowResult.FoundMediaWindow)
            {
                result = $"Could not find {appName} media window.";
            }
            else if (results.FindWindowResult.IsAlreadyFixed)
            {
                result = $"Found {appName} media window. Already fixed.";
            }
            else if (results.IsFixed)
            {
                result = $"Found {appName} media window and fixed it.";
            }
            else if (!results.CoreWindowFocused)
            {
                result = "Could not fix - core window not focused.";
            }
            else
            {
                result = "Could not fix - core window focused.";
            }

            if (result.Equals(_previousMessage))
            {
                return null;
            }
            
            _previousMessage = result;
            return result;
        }
    }
}
