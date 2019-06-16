namespace JwlMediaWin.Core
{
    using System;
    using System.Threading;
    using JwlMediaWin.Core.Models;

    //// Note that "JW Library" is a registered trademark of
    //// Watch Tower Bible and Tract Society of Pennsylvania.

    public class FixerRunner
    {
        private const int IntervalSecsWatchingForProcess = 3;
        private const int IntervalSecsWatchingForWindow = 6;
        private const int IntervalSecsDisabled = 8;

        private bool _started;

        public event EventHandler<FixerStatusEventArgs> StatusEvent;

        public JwLibAppTypes AppType { get; set; }

        public bool KeepOnTop { get; set; }
        
        public void Start()
        {
            if (_started)
            {
                return;
            }

            _started = true;

            var fixer = new Fixer();

            while (true)
            {
                if (AppType == JwLibAppTypes.None)
                {
                    Thread.Sleep(IntervalSecsDisabled * 1000);
                    continue;
                }
                
                var results = fixer.Execute(AppType, KeepOnTop);

                SendStatusEvent(results);

                Thread.Sleep(GetIntervalMilliseconds(results));
            }
        }

        private static int GetIntervalMilliseconds(FixerStatus results)
        {
            if (results.FindWindowResult == null)
            {
                return IntervalSecsWatchingForWindow * 1000;
            }

            return results.FindWindowResult.JwlRunning
                ? IntervalSecsWatchingForProcess * 1000
                : IntervalSecsWatchingForWindow * 1000;
        }

        private void SendStatusEvent(FixerStatus results)
        {
            StatusEvent?.Invoke(this, new FixerStatusEventArgs { Status = results });
        }
    }
}
