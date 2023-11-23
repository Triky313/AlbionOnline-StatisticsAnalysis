using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for DungeonControl.xaml
/// </summary>
public partial class DungeonControl
{
    public DungeonControl()
    {
        InitializeComponent();
    }

    public void ResetDungeons()
    {
        var dialog = new DialogWindow(LanguageController.Translation("RESET_DUNGEON_TRACKER"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_DUNGEON_TRACKER"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is true)
        {
            App.ServiceProvider.GetRequiredService<IDungeonController>()?.ResetDungeons();
        }
    }

    public void ResetTodaysDungeons()
    {
        var dialog = new DialogWindow(LanguageController.Translation("RESET_TODAYS_DUNGEONS"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_DUNGEONS"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is true)
        {
            App.ServiceProvider.GetRequiredService<IDungeonController>()?.ResetDungeonsByDateAscending(DateTime.UtcNow.Date);
        }
    }

    public async Task DeleteSelectedDungeonsAsync()
    {
        var dialog = new DialogWindow(LanguageController.Translation("DELETE_SELECTED_DUNGEONS"), LanguageController.Translation("SURE_YOU_WANT_TO_DELETE_SELECTED_DUNGEONS"));
        var dialogResult = dialog.ShowDialog();

        var vm = (MainWindowViewModelOld) DataContext;

        if (dialogResult is true)
        {
            var selectedDungeons = vm?.DungeonBindings?.Dungeons.Where(x => x.IsSelectedForDeletion ?? false).Select(x => x.DungeonHash);
            if (selectedDungeons != null)
            {
                await App.ServiceProvider.GetRequiredService<IDungeonController>()?.RemoveDungeonByHashAsync(selectedDungeons)!;
            }
        }
    }

    public void DeleteZeroFameDungeons()
    {
        var dialog = new DialogWindow(LanguageController.Translation("DELETE_ZERO_FAME_DUNGEONS"), LanguageController.Translation("SURE_YOU_WANT_TO_DELETE_ZERO_FAME_DUNGEONS"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is true)
        {
            App.ServiceProvider.GetRequiredService<IDungeonController>()?.DeleteDungeonsWithZeroFame();
        }
    }

    #region Ui events

    private void BtnDungeonTrackingReset_Click(object sender, RoutedEventArgs e)
    {
        ResetDungeons();
    }

    private void BtnResetTodaysDungeons_Click(object sender, RoutedEventArgs e)
    {
        ResetTodaysDungeons();
    }

    private void BtnDeleteZeroFameDungeons_Click(object sender, RoutedEventArgs e)
    {
        DeleteZeroFameDungeons();
    }

    private async void BtnDeleteSelectedDungeons_Click(object sender, RoutedEventArgs e)
    {
        await DeleteSelectedDungeonsAsync();
    }

    #endregion
}