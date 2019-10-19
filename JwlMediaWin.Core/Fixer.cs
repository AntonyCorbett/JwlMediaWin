namespace JwlMediaWin.Core
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Windows.Automation;
    using JwlMediaWin.Core.Models;
    using WindowsInput;
    using WindowsInput.Native;

    // ReSharper disable once UnusedMember.Global
    internal class Fixer
    {
        private const string JwLibProcessName = "JWLibrary";
        private const string JwLibCaption = "JW Library";

        private const string JwLibSignLanguageProcessName = "JWLibrary.Forms.UWP";
        private const string JwLibSignLanguageCaption = "JW Library Sign Language";

        private AutomationElement _cachedDesktopElement;
        private MediaAndCoreWindows _cachedWindowElements;

        /// <summary>
        /// Executes the "fixer". Finds the JWL media window and fixes it.
        /// </summary>
        /// <param name="appType">JWL application type.</param>
        /// <param name="topMost">Whether the media window should always be on top.</param>
        /// <returns><see cref="FixerStatus"/></returns>
        /// <exception cref="Exception">Expected app type!</exception>
        // ReSharper disable once UnusedMember.Global
        public FixerStatus Execute(JwLibAppTypes appType, bool topMost)
        {
            try
            {
                string processName;
                string caption;

                switch (appType)
                {
                    case JwLibAppTypes.None:
                        throw new Exception("Expected app type!");

                    default:
                    // ReSharper disable once RedundantCaseLabel
                    case JwLibAppTypes.JwLibrary:
                        processName = JwLibProcessName;
                        caption = JwLibCaption;
                        break;

                    case JwLibAppTypes.JwLibrarySignLanguage:
                        processName = JwLibSignLanguageProcessName;
                        caption = JwLibSignLanguageCaption;
                        break;
                }

                return ExecuteInternal(appType, topMost, processName, caption);
            }
            catch (ElementNotAvailableException)
            {
                return new FixerStatus { ErrorIsTransitioning = true };
            }
            catch (Exception)
            {
                return new FixerStatus { ErrorUnknown = true };
            }
        }

        private static bool ConvertMediaWindow(AutomationElement mainMediaWindow)
        {
            var inputSim = new InputSimulator();

            inputSim.Keyboard.ModifiedKeyStroke(
                new[]
                {
                    VirtualKeyCode.LWIN,
                    VirtualKeyCode.SHIFT
                },
                VirtualKeyCode.RETURN);

            var counter = 0;
            while (!HasTransformPattern(mainMediaWindow) && counter < 20)
            {
                Thread.Sleep(200);
                ++counter;
            }

            return HasTransformPattern(mainMediaWindow);
        }

        private static bool IsCorrectCoreWindow(JwLibAppTypes appType, AutomationElement coreMediaWindow)
        {
            switch (appType)
            {
                default:
                // ReSharper disable once RedundantCaseLabel
                case JwLibAppTypes.None:
                    return false;

                case JwLibAppTypes.JwLibrary:
                    return GetWebView(coreMediaWindow) != null || 
                           HasNoChildren(coreMediaWindow);

                case JwLibAppTypes.JwLibrarySignLanguage:
                    return GetImageControl(coreMediaWindow) != null;
            }
        }

        private static bool HasNoChildren(AutomationElement coreMediaWindow)
        {
            // this is the case when JWL is configured _not_ to show the year text
            return coreMediaWindow.FindFirst(
                       TreeScope.Children, 
                       new PropertyCondition(AutomationElement.IsEnabledProperty, true)) == null;
        }

        private static AutomationElement GetJwlCoreWindow(AutomationElement mainJwlWindow, string caption)
        {
            Condition condition = new AndCondition(
                new PropertyCondition(AutomationElement.NameProperty, caption),
                new PropertyCondition(AutomationElement.ClassNameProperty, "Windows.UI.Core.CoreWindow"));

            return mainJwlWindow.FindFirst(TreeScope.Children, condition);
        }

        private static AutomationElement GetWebView(AutomationElement coreJwlWindow)
        {
            return coreJwlWindow.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "WebView"));
        }

        private static AutomationElement GetImageControl(AutomationElement coreJwlWindow)
        {
            return coreJwlWindow.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.ClassNameProperty, "Image"));
        }

        private static bool HasTransformPattern(AutomationElement item)
        {
            return (bool)item.GetCurrentPropertyValue(AutomationElement.IsTransformPatternAvailableProperty);
        }

        private static bool IsWindowTopMost(AutomationElement item)
        {
            var windowPattern = (WindowPattern)item.GetCurrentPattern(WindowPattern.Pattern);
            return windowPattern.Current.IsTopmost;
        }

        private FixerStatus ExecuteInternal(
            JwLibAppTypes appType, 
            bool topMost,
            string processName, 
            string caption)
        {
            var result = new FixerStatus { FindWindowResult = GetMediaAndCoreWindow(appType, processName, caption) };

            if (!result.FindWindowResult.FoundMediaWindow ||
                result.FindWindowResult.IsAlreadyFixed)
            {
                return result;
            }

            var mainHandle = (IntPtr)result.FindWindowResult.MainMediaWindow.Current.NativeWindowHandle;
            var coreHandle = (IntPtr)result.FindWindowResult.CoreMediaWindow.Current.NativeWindowHandle;

            // this Sleep is probably not needed
            Thread.Sleep(1000);

            NativeMethods.SetForegroundWindow(coreHandle);

            var rect = result.FindWindowResult.MainMediaWindow.Current.BoundingRectangle;

            result.FindWindowResult.CoreMediaWindow.SetFocus();

            if (!result.FindWindowResult.CoreMediaWindow.Current.HasKeyboardFocus)
            {
                return result;
            }

            result.CoreWindowFocused = true;

            // convert the window using Win+Shift+Return
            if (!ConvertMediaWindow(result.FindWindowResult.MainMediaWindow))
            {
                return result;
            }

            var insertAfterValue = topMost
                ? new IntPtr(-1)
                : new IntPtr(0);

            const uint ShowWindowFlag = 0x0040;
            const uint NoCopyBitsFlag = 0x0100;
            const uint NoSendChangingFlag = 0x0400;

            const int adjustment = 34;
            const int border = 8;

            // this Sleep is definitely required. Without it, the media window may remain
            // on the primary display (i.e. the SetWindowPos call is ineffective). See #5
            Thread.Sleep(500);

            NativeMethods.SetWindowPos(
                mainHandle,
                insertAfterValue,
                (int)rect.Left - border,
                (int)rect.Top - adjustment,
                (int)rect.Width + (border * 2),
                (int)rect.Height + adjustment + border,
                (int)(NoCopyBitsFlag | NoSendChangingFlag | ShowWindowFlag));

            result.IsFixed = true;

            return result;
        }

        private FindWindowResult GetMediaAndCoreWindow(
            JwLibAppTypes appType, string processName, string caption)
        {
            var result = new FindWindowResult();

            if (!Process.GetProcessesByName(processName).Any())
            {
                return result;
            }

            result.JwlRunning = true;

            CacheDesktopElement();
            if (_cachedDesktopElement == null)
            {
                return result;
            }

            result.FoundDesktop = true;

            if (_cachedWindowElements == null)
            {
                _cachedWindowElements = GetMediaAndCoreWindowsInternal(appType, caption);
            }

            if (_cachedWindowElements != null)
            {
                result.FoundMediaWindow = true;

                result.CoreMediaWindow = _cachedWindowElements.CoreWindow;
                result.MainMediaWindow = _cachedWindowElements.MediaWindow;

                try
                {
                    // note that the transform property is not available in
                    // the original window; only in one that has been "fixed"
                    if (HasTransformPattern(_cachedWindowElements.MediaWindow))
                    {
                        // already fixed
                        result.IsAlreadyFixed = true;
                    }
                }
                catch (ElementNotAvailableException)
                {
                    // one of the windows has gone away so purge the cache...
                    _cachedWindowElements = null;
                    _cachedDesktopElement = null;

                    return new FindWindowResult
                    {
                        FoundDesktop = true,
                        JwlRunning = true
                    };
                }
            }

            return result;
        }

        private MediaAndCoreWindows GetMediaAndCoreWindowsInternal(JwLibAppTypes appType, string caption)
        {
            var candidates = _cachedDesktopElement.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, caption));

            if (candidates.Count == 0)
            {
                return null;
            }

            foreach (AutomationElement candidate in candidates)
            {
                if (IsWindowTopMost(candidate))
                {
                    var coreWindow = GetJwlCoreWindow(candidate, caption);
                    if (coreWindow != null)
                    {
                        if (IsCorrectCoreWindow(appType, coreWindow))
                        {
                            return new MediaAndCoreWindows
                            {
                                CoreWindow = coreWindow,
                                MediaWindow = candidate
                            };
                        }
                    }
                }
            }

            return null;
        }

        private void CacheDesktopElement()
        {
            if (_cachedDesktopElement == null)
            {
                _cachedDesktopElement = AutomationElement.RootElement;
            }
        }
    }
}
