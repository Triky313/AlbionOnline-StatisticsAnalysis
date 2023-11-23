using Avalonia.Controls;
using StatisticsAnalysisTool.Avalonia.ViewModels;

namespace StatisticsAnalysisTool.Avalonia.UserControls;
public partial class ErrorBarControl : UserControl
{
    public ErrorBarControl()
    {
        InitializeComponent();
        DataContext = new ErrorBarViewModel();
    }
}
