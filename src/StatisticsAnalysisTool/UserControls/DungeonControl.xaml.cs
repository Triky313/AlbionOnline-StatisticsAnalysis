using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System.Linq;
using System.Windows;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for DungeonControl.xaml
    /// </summary>
    public partial class DungeonControl
    {
        public DungeonControl()
        {
            InitializeComponent();
        }

        public void ResetDungeonCounters()
        {
            var vm = (MainWindowViewModel)DataContext;

            vm.DungeonBindings.DungeonStatsTotal.EnteredDungeon = 0;
            vm.DungeonBindings.DungeonStatsTotal.OpenedStandardChests = 0;
            vm.DungeonBindings.DungeonStatsTotal.OpenedUncommonChests = 0;
            vm.DungeonBindings.DungeonStatsTotal.OpenedRareChests = 0;
            vm.DungeonBindings.DungeonStatsTotal.OpenedLegendaryChests = 0;

            vm.DungeonBindings.DungeonStatsDay.EnteredDungeon = 0;
            vm.DungeonBindings.DungeonStatsDay.OpenedStandardChests = 0;
            vm.DungeonBindings.DungeonStatsDay.OpenedUncommonChests = 0;
            vm.DungeonBindings.DungeonStatsDay.OpenedRareChests = 0;
            vm.DungeonBindings.DungeonStatsDay.OpenedLegendaryChests = 0;
        }

        public void ResetDungeons()
        {
            var dialog = new DialogWindow(LanguageController.Translation("RESET_DUNGEON_TRACKER"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_DUNGEON_TRACKER"));
            var dialogResult = dialog.ShowDialog();

            var vm = (MainWindowViewModel)DataContext;

            if (dialogResult is true)
            {
                vm?.TrackingController.DungeonController.ResetDungeons();
            }
        }

        public void DeleteSelectedDungeons()
        {
            var dialog = new DialogWindow(LanguageController.Translation("DELETE_SELECTED_DUNGEONS"), LanguageController.Translation("SURE_YOU_WANT_TO_DELETE_SELECTED_DUNGEONS"));
            var dialogResult = dialog.ShowDialog();

            var vm = (MainWindowViewModel)DataContext;

            if (dialogResult is true)
            {
                var selectedDungeons = vm?.DungeonBindings?.TrackingDungeons.Where(x => x.IsSelectedForDeletion ?? false).Select(x => x.DungeonHash);
                vm?.TrackingController.DungeonController.RemoveDungeonByHashAsync(selectedDungeons);
            }
        }

        #region Ui events

        private void BtnDungeonTrackingReset_Click(object sender, RoutedEventArgs e)
        {
            ResetDungeonCounters();
            ResetDungeons();
        }

        private void BtnDeleteSelectedDungeons_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedDungeons();
        }

        #endregion
    }
}
