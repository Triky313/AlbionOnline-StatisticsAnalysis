using FontAwesome5;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for DungeonControl.xaml
    /// </summary>
    public partial class DungeonControl
    {
        private bool IsDungeonStatsGridUnfold;

        public DungeonControl()
        {
            InitializeComponent();
        }

        public void DungeonStatsGridToggle()
        {
            var unfoldGridHeight = 290;
            var foldGridHeight = 82;

            var vm = (MainWindowViewModel)DataContext;

            if (IsDungeonStatsGridUnfold)
            {
                vm.DungeonStatsGridButtonIcon = EFontAwesomeIcon.Solid_AngleDoubleDown;
                vm.DungeonStatsGridHeight = foldGridHeight;
                vm.DungeonStatsScrollViewerMargin = new Thickness(0, foldGridHeight, 0, 0);
                IsDungeonStatsGridUnfold = false;
            }
            else
            {
                vm.DungeonStatsGridButtonIcon = EFontAwesomeIcon.Solid_AngleDoubleUp;
                vm.DungeonStatsGridHeight = unfoldGridHeight;
                vm.DungeonStatsScrollViewerMargin = new Thickness(0, unfoldGridHeight, 0, 0);
                IsDungeonStatsGridUnfold = true;
            }
        }

        public void ResetDungeonCounters()
        {
            var vm = (MainWindowViewModel)DataContext;

            vm.DungeonStatsTotal.EnteredDungeon = 0;
            vm.DungeonStatsTotal.OpenedStandardChests = 0;
            vm.DungeonStatsTotal.OpenedUncommonChests = 0;
            vm.DungeonStatsTotal.OpenedRareChests = 0;
            vm.DungeonStatsTotal.OpenedLegendaryChests = 0;

            vm.DungeonStatsDay.EnteredDungeon = 0;
            vm.DungeonStatsDay.OpenedStandardChests = 0;
            vm.DungeonStatsDay.OpenedUncommonChests = 0;
            vm.DungeonStatsDay.OpenedRareChests = 0;
            vm.DungeonStatsDay.OpenedLegendaryChests = 0;
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
                var selectedDungeons = vm?.TrackingDungeons.Where(x => x.IsSelectedForDeletion ?? false).Select(x => x.DungeonHash);
                vm?.TrackingController.DungeonController.RemoveDungeonByHashAsync(selectedDungeons);
            }
        }

        #region Ui events

        private void MouseUp_FoldUnfoldDungeonStats(object sender, MouseEventArgs e)
        {
            DungeonStatsGridToggle();
        }

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
