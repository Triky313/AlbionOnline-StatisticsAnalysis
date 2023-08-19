using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Serilog;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for DashboardControl.xaml
/// </summary>
public partial class DashboardControl
{
    public DashboardControl()
    {
        InitializeComponent();
    }

    private void OpenDashboardWindow()
    {
        try
        {
            if (Utilities.IsWindowOpen<DashboardWindow>())
            {
                var existItemWindow = Application.Current.Windows.OfType<DashboardWindow>().FirstOrDefault();
                existItemWindow?.Activate();
            }
            else
            {
                var vm = (MainWindowViewModel) DataContext;
                var itemWindow = new DashboardWindow(vm?.DashboardBindings, vm?.FactionPointStats);
                itemWindow.Show();
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{Message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    #region Ui events

    private void BtnTrackingReset_Click(object sender, RoutedEventArgs e)
    {
        Log.Error("{Message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        var trackingController = ServiceLocator.Resolve<TrackingController>();
        trackingController?.LiveStatsTracker?.Reset();
    }

    private void OpenDashboardWindow_MouseUp(object sender, MouseButtonEventArgs e)
    {
        OpenDashboardWindow();
    }

    #endregion
}