using StatisticsAnalysisTool.Common.UserSettings;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Views.Overlay;

/// <summary>
/// Shared behaviour for the in-game overlays: always on top, no taskbar entry,
/// remembers its position, adjustable background opacity / scale and optional click-through.
/// Left drag moves the overlay, right click closes it (both disabled while click-through is on).
/// </summary>
public abstract class OverlayWindowBase : Window
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x00000020;
    private const int WsExToolWindow = 0x00000080;
    private const int WsExNoActivate = 0x08000000;

    public static readonly DependencyProperty OverlayBackgroundProperty = DependencyProperty.Register(
        nameof(OverlayBackground), typeof(Brush), typeof(OverlayWindowBase), new PropertyMetadata(Brushes.Transparent));

    protected OverlayWindowBase()
    {
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false;
        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.Manual;

        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseRightButtonUp += (_, _) => Close();
        SourceInitialized += (_, _) => ApplySettings();
        Loaded += (_, _) => RestorePosition();
    }

    /// <summary>Semi-transparent background brush; bind the root border's Background to it.</summary>
    public Brush OverlayBackground
    {
        get => (Brush) GetValue(OverlayBackgroundProperty);
        set => SetValue(OverlayBackgroundProperty, value);
    }

    protected abstract (double Left, double Top) GetSavedPosition(SettingsObject settings);
    protected abstract void SavePosition(SettingsObject settings, double left, double top);

    /// <summary>Extra scale for overlays whose content is wide by default (e.g. the damage meter list).</summary>
    protected virtual double BaseScale => 1.0;

    public void ApplySettings()
    {
        var settings = SettingsController.CurrentSettings;

        var opacity = Math.Clamp(settings.OverlayBackgroundOpacityPercentage, 0, 100) / 100d;
        var brush = new SolidColorBrush(Color.FromArgb((byte) Math.Round(opacity * 255), 0x10, 0x12, 0x16));
        brush.Freeze();
        OverlayBackground = brush;

        var scale = Math.Clamp(settings.OverlayScalePercentage, 50, 200) / 100d * BaseScale;
        if (Content is FrameworkElement content)
        {
            content.LayoutTransform = new ScaleTransform(scale, scale);
        }

        SetClickThrough(settings.IsOverlayClickThroughActive);
    }

    private void SetClickThrough(bool isClickThrough)
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var style = GetWindowLong(handle, GwlExStyle) | WsExToolWindow | WsExNoActivate;
        style = isClickThrough ? style | WsExTransparent : style & ~WsExTransparent;
        SetWindowLong(handle, GwlExStyle, style);
    }

    private void RestorePosition()
    {
        var (left, top) = GetSavedPosition(SettingsController.CurrentSettings);
        var virtualLeft = SystemParameters.VirtualScreenLeft;
        var virtualTop = SystemParameters.VirtualScreenTop;
        var virtualRight = virtualLeft + SystemParameters.VirtualScreenWidth;
        var virtualBottom = virtualTop + SystemParameters.VirtualScreenHeight;

        // Keep the overlay reachable if the saved position is off-screen (e.g. monitor removed)
        Left = Math.Clamp(left, virtualLeft, Math.Max(virtualLeft, virtualRight - 60));
        Top = Math.Clamp(top, virtualTop, Math.Max(virtualTop, virtualBottom - 40));
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed)
        {
            return;
        }

        DragMove();
        SavePosition(SettingsController.CurrentSettings, Left, Top);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        SavePosition(SettingsController.CurrentSettings, Left, Top);
        base.OnClosing(e);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}
