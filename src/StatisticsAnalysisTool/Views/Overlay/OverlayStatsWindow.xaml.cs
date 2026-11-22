using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.Views.Overlay;

public partial class OverlayStatsWindow
{
    private static readonly Brush ArmedBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x5C));
    private readonly DispatcherTimer _disarmTimer = new() { Interval = TimeSpan.FromSeconds(3) };
    private Brush _resetIconDefaultBrush;
    private bool _isResetArmed;

    public OverlayStatsWindow(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        DataContext = mainWindowViewModel;
        _disarmTimer.Tick += (_, _) => DisarmReset();
    }

    protected override (double Left, double Top) GetSavedPosition(SettingsObject settings)
        => (settings.OverlayStatsLeft, settings.OverlayStatsTop);

    protected override void SavePosition(SettingsObject settings, double left, double top)
    {
        settings.OverlayStatsLeft = left;
        settings.OverlayStatsTop = top;
    }

    private async void ResetButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Handled so the click does not start dragging the overlay
        e.Handled = true;

        if (!_isResetArmed)
        {
            _isResetArmed = true;
            _resetIconDefaultBrush ??= ResetIcon.Foreground;
            ResetIcon.Foreground = ArmedBrush;
            ResetButton.ToolTip = OverlayTranslation.ResetSessionConfirm;
            _disarmTimer.Start();
            return;
        }

        DisarmReset();
        try
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            await trackingController.StatisticController.ResetSessionAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Overlay session reset failed");
        }
    }

    private void DisarmReset()
    {
        _disarmTimer.Stop();
        _isResetArmed = false;
        if (_resetIconDefaultBrush != null)
        {
            ResetIcon.Foreground = _resetIconDefaultBrush;
        }

        ResetButton.ToolTip = OverlayTranslation.ResetSessionHint;
    }
}
