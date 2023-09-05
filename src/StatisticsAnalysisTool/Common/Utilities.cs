using Serilog;
using StatisticsAnalysisTool.Notification;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace StatisticsAnalysisTool.Common;

public static class Utilities
{
    public static long GetHighestLength(params Array[] arrays)
    {
        long highestLength = 0;

        foreach (var array in arrays)
        {
            if (array.Length > highestLength)
            {
                highestLength = array.Length;
            }
        }

        return highestLength;
    }

    public static void CenterWindowOnScreen(Window window)
    {
        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        var windowWidth = window.Width;
        var windowHeight = window.Height;
        window.Left = (screenWidth / 2) - (windowWidth / 2);
        window.Top = (screenHeight / 2) - (windowHeight / 2);
    }

    public static bool IsWindowOpen<T>(string name = "") where T : Window
    {
        return string.IsNullOrEmpty(name)
            ? Application.Current.Windows.OfType<T>().Any()
            : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
    }

    public static double GetValuePerSecondToDouble(double value, DateTime? combatStart, TimeSpan time, double maxValue = -1)
    {
        if (double.IsInfinity(value)) return maxValue > 0 ? maxValue : double.MaxValue;

        if (time.Ticks <= 1 && combatStart != null)
        {
            var startTimeSpan = DateTime.UtcNow - (DateTime) combatStart;
            var calculation = value / startTimeSpan.TotalSeconds;
            return calculation > maxValue ? maxValue : calculation;
        }

        var valuePerSeconds = value / time.TotalSeconds;
        if (maxValue > 0 && valuePerSeconds > maxValue) return maxValue;

        return valuePerSeconds;
    }

    public static bool IsBlockingTimeExpired(DateTime dateTime, int waitingSeconds)
    {
        var currentDateTime = DateTime.UtcNow;
        var difference = currentDateTime.Subtract(dateTime);
        return difference.Seconds >= waitingSeconds;
    }

    public static void AnotherAppToStart(string path)
    {
        var notifyManager = ServiceLocator.Resolve<SatNotificationManager>();

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            if (!File.Exists(path))
            {
                notifyManager?.ShowErrorAsync(LanguageController.Translation("CANNOT_START_OTHER_APP"),
                    LanguageController.Translation("CAN_NOT_START_APP_WITH_PATH",
                        new List<string> { "path" },
                        new List<string> { path }));
                return;
            }

            Process.Start(path);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            notifyManager?.ShowErrorAsync(LanguageController.Translation("CANNOT_START_OTHER_APP"),
                LanguageController.Translation("CAN_NOT_START_APP_WITH_PATH",
                    new List<string> { "path" },
                    new List<string> { path }));
        }
    }

    #region Window Flash

    private const uint FlashwStop = 0; //Stop flashing. The system restores the window to its original state.
    // ReSharper disable once UnusedMember.Local
    private const uint FlashwCaption = 1; //Flash the window caption.        
    // ReSharper disable once UnusedMember.Local
    private const uint FlashwTray = 2; //Flash the taskbar button.        
    private const uint FlashwAll = 3; //Flash both the window caption and taskbar button.        
    private const uint FlashwTimer = 4; //Flash continuously, until the FLASHW_STOP flag is set.        
    // ReSharper disable once UnusedMember.Local
    private const uint FlashwTimernofg = 12; //Flash continuously until the window comes to the foreground.  

    [StructLayout(LayoutKind.Sequential)]
    private struct FlashInfo
    {
        public uint cbSize; //The size of the structure in bytes.            
        public IntPtr hwnd; //A Handle to the Window to be Flashed. The window can be either opened or minimized.

        public uint dwFlags; //The Flash Status.            
        public uint uCount; // number of times to flash the window            

        public uint
            dwTimeout; //The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.        
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlashWindowEx(ref FlashInfo pwfi);

    public static void FlashWindow(this Window win, uint count = uint.MaxValue)
    {
        win.Dispatcher.Invoke(() =>
        {
            if (win.IsActive) return;

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