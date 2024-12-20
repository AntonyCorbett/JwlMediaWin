﻿namespace JwlMediaWin.Core
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

        private static bool HasWindowPattern(AutomationElement item)
        {
            return (bool)item.GetCurrentPropertyValue(AutomationElement.IsWindowPatternAvailableProperty);
        }

        private static bool IsWindowTopMost(AutomationElement item)
        {
            if (!HasWindowPattern(item))
            {
                return false;
            }

            if (item.GetCurrentPattern(WindowPattern.Pattern) is WindowPattern wp)
            {
                return wp.Current.IsTopmost;
            }

            return false;
        }
        
        private static bool IsAJwlWindow(AutomationElement item)
        {
            return item.Current.Name?.Contains(JwLibCaption) ?? false;
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
                : new IntPtr(-2);

            const uint ShowWindowFlag = 0x0040;
            const uint NoCopyBitsFlag = 0x0100;
            const uint NoSendChangingFlag = 0x0400;

            // the window used to have a non-transparent titlebar so we could
            // just remove it by trimming the top margin...
            // const int adjustment = 34; // adjustment for titlebar
            // const int border = 8; // adjustment for borders

            // ... but in Jan 2021 the title bar became transparent and so 
            // it needed to be retained. However, the command buttons top right
            // can be 'removed' (see below)
            const int adjustment = 1; // adjustment for titlebar
            const int border = 8; // adjustment for borders

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

            EnsureWindowIsNonSizeable(mainHandle);

            // to 'remove' the command buttons from the transparent title bar
            // we move focus to the main JWL window and then disable the media
            // window meaning it can no longer gain focus via mouse click. This
            // doesn't prevent the button being displayed if you alt-tab to the
            // media window but it is a small change that is generally helpful.

            NativeMethods.SetForegroundWindow(coreHandle);
            NativeMethods.EnableWindow(mainHandle, false);

            return result;
        }

        private void EnsureWindowIsNonSizeable(IntPtr mainHandle)
        {
            const int GWL_STYLE = -16;
            const int WS_SIZEBOX = 0x040000;

            var val = (int)NativeMethods.GetWindowLongPtr(mainHandle, GWL_STYLE) & ~WS_SIZEBOX;
            NativeMethods.SetWindowLongPtr(mainHandle, GWL_STYLE, (IntPtr)val);
        }

        private FindWindowResult GetMediaAndCoreWindow(
            JwLibAppTypes appType, string processName, string caption)
        {
            var result = new FindWindowResult();

            if (Process.GetProcessesByName(processName).Length == 0)
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
            // get all direct child windows of the desktop
            var candidateMediaWindows = _cachedDesktopElement.FindAll(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.IsEnabledProperty, true));

            if (candidateMediaWindows.Count == 0)
            {
                return null;
            }

            foreach (AutomationElement candidateMediaWindow in candidateMediaWindows)
            {
                // the media window is topmost and has "JW Library" in its Name
                if (IsWindowTopMost(candidateMediaWindow) && IsAJwlWindow(candidateMediaWindow))
                {
                    var coreWindow = GetJwlCoreWindow(candidateMediaWindow, caption);
                    if (coreWindow != null)
                    {
                        if (IsCorrectCoreWindow(appType, coreWindow))
                        {
                            return new MediaAndCoreWindows
                            {
                                CoreWindow = coreWindow,
                                MediaWindow = candidateMediaWindow
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
