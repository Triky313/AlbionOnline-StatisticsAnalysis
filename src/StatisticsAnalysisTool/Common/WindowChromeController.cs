using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Forms = System.Windows.Forms;

namespace StatisticsAnalysisTool.Common;

public sealed class WindowChromeController
{
    private const int RestoreButtonContent = 2;
    private const int MaximizeButtonContent = 1;

    private readonly Window _window;
    private readonly Button _maximizeButton;
    private readonly ResizeMode _restoredResizeMode;
    private readonly ResizeMode? _maximizedResizeMode;
    private readonly bool _centerOnRestore;
    private readonly bool _topmostOnMaximize;
    private bool _isMaximized;

    public WindowChromeController(
        Window window,
        Button maximizeButton,
        ResizeMode restoredResizeMode,
        ResizeMode? maximizedResizeMode = null,
        bool centerOnRestore = false,
        bool topmostOnMaximize = false)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _maximizeButton = maximizeButton ?? throw new ArgumentNullException(nameof(maximizeButton));
        _restoredResizeMode = restoredResizeMode;
        _maximizedResizeMode = maximizedResizeMode;
        _centerOnRestore = centerOnRestore;
        _topmostOnMaximize = topmostOnMaximize;
        _isMaximized = _window.WindowState == WindowState.Maximized;

        UpdateMaximizeButtonContent();
        _window.StateChanged += Window_StateChanged;
    }

    public void DragMoveOnMouseDown(MouseButtonEventArgs e)
    {
        if (e is null)
        {
            return;
        }

        if (e.ChangedButton != MouseButton.Left || e.ButtonState != MouseButtonState.Pressed || e.ClickCount != 1)
        {
            return;
        }

        try
        {
            _window.DragMove();
        }
        catch (InvalidOperationException ex)
        {
            Log.Debug(ex, "Window drag move was ignored because WPF rejected the current mouse state.");
        }
    }

    public void ToggleMaximizeOnDoubleClick(MouseButtonEventArgs e)
    {
        if (e is null)
        {
            return;
        }

        if (e.ChangedButton != MouseButton.Left || e.ClickCount != 2)
        {
            return;
        }

        ToggleMaximize();
        e.Handled = true;
    }

    public void ToggleMaximize()
    {
        if (_isMaximized)
        {
            Restore();
            return;
        }

        Maximize();
    }

    private void Maximize()
    {
        var windowInteropHelper = new WindowInteropHelper(_window);
        var screen = Forms.Screen.FromHandle(windowInteropHelper.Handle);

        _window.MaxHeight = screen.WorkingArea.Height;

        if (_maximizedResizeMode.HasValue)
        {
            _window.ResizeMode = _maximizedResizeMode.Value;
        }

        if (_topmostOnMaximize)
        {
            _window.Topmost = true;
        }

        _window.WindowState = WindowState.Maximized;
        _isMaximized = true;
        UpdateMaximizeButtonContent();
    }

    private void Restore()
    {
        _window.WindowState = WindowState.Normal;
        _window.ResizeMode = _restoredResizeMode;
        _isMaximized = false;

        if (_topmostOnMaximize)
        {
            _window.Topmost = false;
        }

        if (_centerOnRestore)
        {
            Utilities.CenterWindowOnScreen(_window);
        }

        UpdateMaximizeButtonContent();
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        _isMaximized = _window.WindowState == WindowState.Maximized;
        UpdateMaximizeButtonContent();
    }

    private void UpdateMaximizeButtonContent()
    {
        _maximizeButton.Content = _isMaximized
            ? RestoreButtonContent
            : MaximizeButtonContent;
    }
}