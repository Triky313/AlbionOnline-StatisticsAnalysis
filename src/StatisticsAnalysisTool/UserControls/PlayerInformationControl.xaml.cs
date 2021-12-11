using System.Windows;
using StatisticsAnalysisTool.ViewModels;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for PlayerInformationControl.xaml
    /// </summary>
    public partial class PlayerInformationControl
    {
        private readonly PlayerInformationViewModel _playerInformationViewModel;

        public PlayerInformationControl()
        {
            InitializeComponent();
            _playerInformationViewModel = new PlayerInformationViewModel();
            DataContext = _playerInformationViewModel;
        }

        private async void BtnPlayerModeSave_Click(object sender, RoutedEventArgs e)
        {
            await _playerInformationViewModel.SetComparedPlayerModeInfoValues();
        }

        private async void TxtBoxPlayerModeUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            await _playerInformationViewModel.SetComparedPlayerModeInfoValues();
        }
    }
}
