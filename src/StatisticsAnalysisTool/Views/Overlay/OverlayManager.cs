using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StatisticsAnalysisTool.Views.Overlay;

/// <summary>
/// Opens / closes the three in-game overlays (stats, damage meter, gathering)
/// and pushes settings changes to the ones that are open.
/// </summary>
public static class OverlayManager
{
    private const int HotkeyId = 0x5A71;
    private const int WmHotkey = 0x0312;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModNoRepeat = 0x4000;
    private const uint VkO = 0x4F;

    /// <summary>
    /// Registers Ctrl+Shift+O as a system-wide hotkey that toggles the overlays,
    /// so they can be shown / hidden while the game has focus.
    /// </summary>
    public static void RegisterHotkey(Window mainWindow, MainWindowViewModel mainWindowViewModel)
    {
        StartAutoHide();

        var handle = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
        if (handle == System.IntPtr.Zero)
        {
            return;
        }

        if (!RegisterHotKey(handle, HotkeyId, ModControl | ModShift | ModNoRepeat, VkO))
        {
            Serilog.Log.Warning("Overlay hotkey Ctrl+Shift+O could not be registered (already used by another program)");
            return;
        }

        System.Windows.Interop.HwndSource.FromHwnd(handle)?.AddHook((System.IntPtr _, int msg, System.IntPtr wParam, System.IntPtr _, ref bool handled) =>
        {
            if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
            {
                Toggle(mainWindowViewModel);
                handled = true;
            }

            return System.IntPtr.Zero;
        });
        mainWindow.Closed += (_, _) => UnregisterHotKey(handle, HotkeyId);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool RegisterHotKey(System.IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(System.IntPtr hWnd, int id);

    #region Auto-hide

    private static readonly System.Windows.Threading.DispatcherTimer AutoHideTimer = new() { Interval = System.TimeSpan.FromMilliseconds(400) };
    private static readonly int OwnProcessId = System.Environment.ProcessId;
    private static int _lastForegroundProcessId = -1;
    private static bool _lastForegroundAllowed = true;

    /// <summary>
    /// Every 400 ms: overlays are only visible while Albion Online (or this tool) is the foreground window,
    /// so they do not cover other programs. Can be turned off in the settings.
    /// </summary>
    private static void StartAutoHide()
    {
        if (AutoHideTimer.IsEnabled)
        {
            return;
        }

        AutoHideTimer.Tick += (_, _) => UpdateAutoHide();
        AutoHideTimer.Start();
    }

    private static void UpdateAutoHide()
    {
        var overlays = OpenOverlays.ToList();
        if (overlays.Count == 0)
        {
            return;
        }

        var shouldShow = !SettingsController.CurrentSettings.IsOverlayAutoHideActive || IsGameOrToolInForeground();
        var visibility = shouldShow ? Visibility.Visible : Visibility.Hidden;
        foreach (var overlay in overlays.Where(x => x.Visibility != visibility))
        {
            overlay.Visibility = visibility;
        }
    }

    private static bool IsGameOrToolInForeground()
    {
        var foreground = GetForegroundWindow();
        if (foreground == System.IntPtr.Zero)
        {
            return _lastForegroundAllowed;
        }

        GetWindowThreadProcessId(foreground, out var processId);
        if (processId == _lastForegroundProcessId)
        {
            return _lastForegroundAllowed;
        }

        _lastForegroundProcessId = processId;
        try
        {
            using var process = System.Diagnostics.Process.GetProcessById(processId);
            _lastForegroundAllowed = processId == OwnProcessId
                                     || process.ProcessName.StartsWith("Albion-Online", System.StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            _lastForegroundAllowed = false;
        }

        return _lastForegroundAllowed;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern System.IntPtr GetForegroundWindow();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(System.IntPtr hWnd, out int processId);

    #endregion

    private static IEnumerable<OverlayWindowBase> OpenOverlays =>
        Application.Current.Windows.OfType<OverlayWindowBase>().ToList();

    public static bool IsAnyOpen => OpenOverlays.Any();

    /// <summary>Closes all overlays if any is open, otherwise opens every enabled overlay.</summary>
    public static void Toggle(MainWindowViewModel mainWindowViewModel)
    {
        if (IsAnyOpen)
        {
            CloseAll();
            return;
        }

        OpenEnabled(mainWindowViewModel);
    }

    public static void CloseAll()
    {
        foreach (var overlay in OpenOverlays)
        {
            overlay.Close();
        }
    }

    /// <summary>Re-applies opacity / scale / click-through and opens or closes overlays that were (de)activated.</summary>
    public static void ApplySettings(MainWindowViewModel mainWindowViewModel)
    {
        if (!IsAnyOpen)
        {
            return;
        }

        var settings = SettingsController.CurrentSettings;
        SyncOverlay<OverlayStatsWindow>(settings.IsOverlayStatsEnabled, () => new OverlayStatsWindow(mainWindowViewModel));
        SyncOverlay<OverlayDamageMeterWindow>(settings.IsOverlayDamageMeterEnabled, () => new OverlayDamageMeterWindow(mainWindowViewModel));
        SyncOverlay<OverlayGatheringWindow>(settings.IsOverlayGatheringEnabled, () => new OverlayGatheringWindow(mainWindowViewModel));

        foreach (var overlay in OpenOverlays)
        {
            overlay.ApplySettings();
        }
    }

    private static void OpenEnabled(MainWindowViewModel mainWindowViewModel)
    {
        var settings = SettingsController.CurrentSettings;
        if (settings.IsOverlayStatsEnabled)
        {
            new OverlayStatsWindow(mainWindowViewModel).Show();
        }

        if (settings.IsOverlayDamageMeterEnabled)
        {
            new OverlayDamageMeterWindow(mainWindowViewModel).Show();
        }

        if (settings.IsOverlayGatheringEnabled)
        {
            new OverlayGatheringWindow(mainWindowViewModel).Show();
        }
    }

    private static void SyncOverlay<T>(bool isEnabled, System.Func<T> create) where T : OverlayWindowBase
    {
        var existing = Application.Current.Windows.OfType<T>().FirstOrDefault();
        if (isEnabled && existing == null)
        {
            create().Show();
        }
        else if (!isEnabled)
        {
            existing?.Close();
        }
    }
}
