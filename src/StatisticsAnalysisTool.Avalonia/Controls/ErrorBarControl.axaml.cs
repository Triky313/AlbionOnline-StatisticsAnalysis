using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.Avalonia.ViewModels;

namespace StatisticsAnalysisTool.Avalonia.Controls;
public partial class ErrorBarControl : UserControl
{
    public ErrorBarControl()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider?.GetRequiredService<ErrorBarViewModel>();
    }
}
