using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views;
/// <summary>
/// Interaction logic for GameDataPreparationWindow.xaml
/// </summary>
public partial class GameDataPreparationWindow
{
    private readonly GameDataPreparationWindowViewModel _gameDataPreparationWindowViewModel;

    public GameDataPreparationWindow()
    {
        InitializeComponent();
        _gameDataPreparationWindowViewModel = new GameDataPreparationWindowViewModel();
        DataContext = _gameDataPreparationWindowViewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void BtnSelectMainGameFolder_Click(object sender, RoutedEventArgs e)
    {
        _gameDataPreparationWindowViewModel.OpenPathSelection();
    }

    private void BtnConfirm_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
