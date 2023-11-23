using FontAwesome5;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

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
                var vm = (MainWindowViewModelOld) DataContext;
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

    private void BtnTrackingReset_Click(object sender, RoutedEventArgs e)
    {
        Log.Error("{Message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        App.ServiceProvider.GetRequiredService<ILiveStatsTracker>()?.Reset();
    }

    private void OpenDashboardWindow_MouseUp(object sender, MouseButtonEventArgs e)
    {
        OpenDashboardWindow();
    }

    private void KillDeathToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModelOld) DataContext;
        if (vm.DashboardBindings.KillDeathStatsVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.KillDeathStatsVisibility = Visibility.Collapsed;
            vm.DashboardBindings.KillDeathStatsToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.KillDeathStatsVisibility = Visibility.Visible;
            vm.DashboardBindings.KillDeathStatsToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }

    private void LootedChestsToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModelOld) DataContext;
        if (vm.DashboardBindings.LootedChestsStatsVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.LootedChestsStatsVisibility = Visibility.Collapsed;
            vm.DashboardBindings.LootedChestsStatsToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.LootedChestsStatsVisibility = Visibility.Visible;
            vm.DashboardBindings.LootedChestsStatsToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }

    private void ReSpecStatsToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModelOld) DataContext;
        if (vm.DashboardBindings.ReSpecStatsVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.ReSpecStatsVisibility = Visibility.Collapsed;
            vm.DashboardBindings.ReSpecStatsToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.ReSpecStatsVisibility = Visibility.Visible;
            vm.DashboardBindings.ReSpecStatsToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }

    private void RepairCostsStatsToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModelOld) DataContext;
        if (vm.DashboardBindings.RepairCostsStatsVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.RepairCostsStatsVisibility = Visibility.Collapsed;
            vm.DashboardBindings.RepairCostsStatsToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.RepairCostsStatsVisibility = Visibility.Visible;
            vm.DashboardBindings.RepairCostsStatsToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }
}