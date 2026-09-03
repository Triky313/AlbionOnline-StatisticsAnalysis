using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using WpfApplication = System.Windows.Application;

namespace StatisticsAnalysisTool.Common;

public sealed class SystemTrayService : IDisposable
{
    private const string ApplicationName = "Statistics Analysis Tool";

    private readonly Window _window;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly Icon _applicationIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly ToolStripMenuItem _openMenuItem;
    private readonly ToolStripMenuItem _trackingMenuItem;
    private readonly ToolStripMenuItem _exitMenuItem;
    private readonly NotifyIcon _notifyIcon;
    private WindowState _windowStateBeforeMinimizing = WindowState.Normal;
    private bool _isWindowHidden;
    private bool _hasShownMinimizedNotification;
    private bool _isTrackingStateChanging;
    private bool _isDisposed;

    public SystemTrayService(Window window, MainWindowViewModel mainWindowViewModel)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _mainWindowViewModel = mainWindowViewModel ?? throw new ArgumentNullException(nameof(mainWindowViewModel));
        _windowStateBeforeMinimizing = window.WindowState;
        _applicationIcon = LoadApplicationIcon();
        _openMenuItem = new ToolStripMenuItem();
        _trackingMenuItem = new ToolStripMenuItem();
        _exitMenuItem = new ToolStripMenuItem();
        _contextMenu = new ContextMenuStrip
        {
            BackColor = SystemTrayMenuRenderer.BackgroundColor,
            ForeColor = SystemTrayMenuRenderer.ForegroundColor,
            Renderer = new SystemTrayMenuRenderer(),
            ShowCheckMargin = false,
            ShowImageMargin = false
        };
        _contextMenu.Items.Add(_openMenuItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(_trackingMenuItem);
        _contextMenu.Items.Add(new ToolStripSeparator());
        _contextMenu.Items.Add(_exitMenuItem);
        _notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = _contextMenu,
            Icon = _applicationIcon,
            Text = ApplicationName,
            Visible = false
        };

        RefreshLocalizedText();
        _window.StateChanged += Window_OnStateChanged;
        _contextMenu.Opening += ContextMenu_OnOpening;
        _openMenuItem.Click += OpenMenuItem_OnClick;
        _trackingMenuItem.Click += TrackingMenuItem_OnClick;
        _exitMenuItem.Click += ExitMenuItem_OnClick;
        _notifyIcon.DoubleClick += NotifyIcon_OnDoubleClick;
        _notifyIcon.BalloonTipClicked += NotifyIcon_OnBalloonTipClicked;
    }

    public WindowState WindowStateForPersistence => _isWindowHidden
        ? _windowStateBeforeMinimizing
        : _window.WindowState;

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _window.StateChanged -= Window_OnStateChanged;
        _contextMenu.Opening -= ContextMenu_OnOpening;
        _openMenuItem.Click -= OpenMenuItem_OnClick;
        _trackingMenuItem.Click -= TrackingMenuItem_OnClick;
        _exitMenuItem.Click -= ExitMenuItem_OnClick;
        _notifyIcon.DoubleClick -= NotifyIcon_OnDoubleClick;
        _notifyIcon.BalloonTipClicked -= NotifyIcon_OnBalloonTipClicked;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
        _applicationIcon.Dispose();
    }

    private void Window_OnStateChanged(object sender, EventArgs eventArgs)
    {
        if (_window.WindowState != WindowState.Minimized)
        {
            _windowStateBeforeMinimizing = _window.WindowState;
            return;
        }

        if (!SettingsController.CurrentSettings.IsMinimizeToSystemTrayActive)
        {
            return;
        }

        MinimizeToSystemTray();
    }

    private void MinimizeToSystemTray()
    {
        _isWindowHidden = true;
        RefreshLocalizedText();
        _notifyIcon.Visible = true;
        _window.Hide();

        if (_hasShownMinimizedNotification)
        {
            return;
        }

        _hasShownMinimizedNotification = true;
        _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
        _notifyIcon.BalloonTipTitle = ApplicationName;
        _notifyIcon.BalloonTipText = SystemTrayTranslation.ApplicationContinuesToRunInSystemTray;
        _notifyIcon.ShowBalloonTip(3000);
    }

    private void RestoreWindow()
    {
        if (_isDisposed)
        {
            return;
        }

        if (!_window.Dispatcher.CheckAccess())
        {
            _window.Dispatcher.BeginInvoke((Action) RestoreWindow);
            return;
        }

        _isWindowHidden = false;
        _notifyIcon.Visible = false;
        _window.WindowState = _windowStateBeforeMinimizing;
        _window.Show();
        _window.Activate();
    }

    private void ExitApplication()
    {
        if (_isDisposed)
        {
            return;
        }

        if (!_window.Dispatcher.CheckAccess())
        {
            _window.Dispatcher.BeginInvoke((Action) ExitApplication);
            return;
        }

        _notifyIcon.Visible = false;
        WpfApplication.Current.Shutdown();
    }

    private void RefreshLocalizedText()
    {
        _openMenuItem.Text = SystemTrayTranslation.OpenApplication;
        _trackingMenuItem.Text = _mainWindowViewModel.IsTrackingActive
            ? SystemTrayTranslation.DeactivateTracking
            : SystemTrayTranslation.ActivateTracking;
        _trackingMenuItem.Enabled = !_isTrackingStateChanging
                                    && ServiceLocator.IsServiceInDictionary<TrackingController>();
        _exitMenuItem.Text = SystemTrayTranslation.ExitApplication;
    }

    private async Task ToggleTrackingAsync()
    {
        if (_isTrackingStateChanging
            || !ServiceLocator.IsServiceInDictionary<TrackingController>())
        {
            return;
        }

        _isTrackingStateChanging = true;
        RefreshLocalizedText();

        try
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            if (_mainWindowViewModel.IsTrackingActive)
            {
                trackingController.StopTracking();
            }
            else
            {
                await trackingController.StartTrackingAsync();
            }
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Tracking could not be toggled from the system tray");
        }
        finally
        {
            _isTrackingStateChanging = false;
            RefreshLocalizedText();
        }
    }
    private void ContextMenu_OnOpening(object sender, CancelEventArgs eventArgs)
    {
        RefreshLocalizedText();
    }

    private void OpenMenuItem_OnClick(object sender, EventArgs eventArgs)
    {
        RestoreWindow();
    }

    private async void TrackingMenuItem_OnClick(object sender, EventArgs eventArgs)
    {
        await ToggleTrackingAsync();
    }

    private void ExitMenuItem_OnClick(object sender, EventArgs eventArgs)
    {
        ExitApplication();
    }

    private void NotifyIcon_OnDoubleClick(object sender, EventArgs eventArgs)
    {
        RestoreWindow();
    }

    private void NotifyIcon_OnBalloonTipClicked(object sender, EventArgs eventArgs)
    {
        RestoreWindow();
    }

    private static Icon LoadApplicationIcon()
    {
        try
        {
            var processPath = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(processPath))
            {
                var icon = Icon.ExtractAssociatedIcon(processPath);
                if (icon != null)
                {
                    return icon;
                }
            }
        }
        catch (Exception exception)
        {
            Log.Warning(exception, "The application icon could not be loaded for the system tray");
        }

        return (Icon) SystemIcons.Application.Clone();
    }
}
