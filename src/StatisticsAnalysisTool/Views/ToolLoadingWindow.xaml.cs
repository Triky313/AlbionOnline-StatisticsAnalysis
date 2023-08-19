using StatisticsAnalysisTool.ViewModels;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views;
/// <summary>
/// Interaction logic for ToolLoadingWindow.xaml
/// </summary>
public partial class ToolLoadingWindow
{
    public ToolLoadingWindow(ToolLoadingWindowViewModel toolLoadingWindowViewModel)
    {
        InitializeComponent();
        DataContext = toolLoadingWindowViewModel;
    }

    private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }
}
