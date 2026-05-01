using StatisticsAnalysisTool.ViewModels;
using System.Windows.Controls;

namespace StatisticsAnalysisTool.UserControls;

public partial class GameDataControl
{
    private readonly GameDataViewModel _gameDataViewModel = new();

    public GameDataControl()
    {
        InitializeComponent();
        DataContext = _gameDataViewModel;
        Loaded += GameDataControl_Loaded;
    }

    private async void GameDataControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Loaded -= GameDataControl_Loaded;
        await _gameDataViewModel.LoadLootChestsAsync().ConfigureAwait(false);
    }
}
