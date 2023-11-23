using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for ErrorBarControl.xaml
/// </summary>
public partial class ErrorBarControl
{
    public ErrorBarControl()
    {
        InitializeComponent();
        var errorBarViewModel = App.ServiceProvider.GetRequiredService<ErrorBarViewModel>();
        DataContext = errorBarViewModel;
    }
}