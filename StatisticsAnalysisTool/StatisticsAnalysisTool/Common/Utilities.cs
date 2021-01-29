using AutoUpdaterDotNET;
using Microsoft.Win32;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace StatisticsAnalysisTool.Common
{
    public static class Utilities
    {
        public static void AutoUpdate()
        {
            AutoUpdater.Start(Settings.Default.AutoUpdateConfigUrl);
            AutoUpdater.DownloadPath = Environment.CurrentDirectory;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
        }

        private static void AutoUpdater_ApplicationExitEvent()
        {
            Thread.Sleep(5000);
            Application.Current.Shutdown();
        }

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
                ? Application.Current.Windows.OfType<T>().Any()
                : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        public static string UlongMarketPriceToString(ulong value) => value.ToString("N0", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));

        public static string MarketPriceDateToString(DateTime value) => Formatting.CurrentDateTimeFormat(value);

        public static bool IsSoftwareInstalled(string c_name)
        {
            string displayName;

            var registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            var key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (var subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey?.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(c_name))
                    {
                        return true;
                    }
                }
                key.Close();
            }

            registryKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (var subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey?.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(c_name))
                    {
                        return true;
                    }
                }
                key.Close();
            }
            return false;
        }

        public static Dictionary<int, T> ToDictionary<T>(this IEnumerable<T> array)
        {
            return array
                .Select((v, i) => new { Key = i, Value = v })
                .ToDictionary(o => o.Key, o => o.Value);
        }

        public static bool IsArrayOf<T>(this Type type)
        {
            return type == typeof(T[]);
        }

        #region Window Flash

        private const uint FlashwStop = 0; //Stop flashing. The system restores the window to its original state.
        private const uint FlashwCaption = 1; //Flash the window caption.        
        private const uint FlashwTray = 2; //Flash the taskbar button.        
        private const uint FlashwAll = 3; //Flash both the window caption and taskbar button.        
        private const uint FlashwTimer = 4; //Flash continuously, until the FLASHW_STOP flag is set.        
        private const uint FlashwTimernofg = 12; //Flash continuously until the window comes to the foreground.  

        [StructLayout(LayoutKind.Sequential)]
        private struct FlashInfo
        {
            public uint cbSize; //The size of the structure in bytes.            
            public IntPtr hwnd; //A Handle to the Window to be Flashed. The window can be either opened or minimized.


            public uint dwFlags; //The Flash Status.            
            public uint uCount; // number of times to flash the window            
            public uint dwTimeout; //The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.        
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FlashInfo pwfi);

        public static void FlashWindow(this Window win, uint count = uint.MaxValue)
        {
            win.Dispatcher.Invoke(() =>
            {
                if (win.IsActive)
                {
                    return;
                }

                var h = new WindowInteropHelper(win);

                var info = new FlashInfo
                {
                    hwnd = h.Handle,
                    dwFlags = FlashwAll | FlashwTimer,
                    uCount = count,
                    dwTimeout = 0
                };

                info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
                FlashWindowEx(ref info);
            });
        }

        public static void StopFlashingWindow(this Window win)
        {
            win.Dispatcher.Invoke(() =>
            {
                var h = new WindowInteropHelper(win);
                var info = new FlashInfo { hwnd = h.Handle };
                info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
                info.dwFlags = FlashwStop;
                info.uCount = uint.MaxValue;
                info.dwTimeout = 0;
                FlashWindowEx(ref info);
            });
        }

        #endregion
    }
}