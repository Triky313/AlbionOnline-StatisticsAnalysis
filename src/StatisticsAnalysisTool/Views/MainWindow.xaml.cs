using StatisticsAnalysisTool.ViewModels;
using System.ComponentModel;

namespace StatisticsAnalysisTool.Views;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    public MainWindow(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        _mainWindowViewModel = mainWindowViewModel;
        DataContext = mainWindowViewModel;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        _mainWindowViewModel.ClosingWindow();
    }
}