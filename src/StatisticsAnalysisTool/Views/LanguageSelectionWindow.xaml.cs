using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views;
/// <summary>
/// Interaction logic for GameDataPreparationWindow.xaml
/// </summary>
public partial class LanguageSelectionWindow
{
    private readonly LanguageSelectionWindowViewModel _languageSelectionWindowViewModel;

    public LanguageSelectionWindow()
    {
        InitializeComponent();
        _languageSelectionWindowViewModel = new LanguageSelectionWindowViewModel();
        DataContext = _languageSelectionWindowViewModel;
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

    private void BtnConfirm_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_languageSelectionWindowViewModel.SelectedFileInformation.FileName))
        {
            return;
        }

        DialogResult = true;
        Close();
    }
}
