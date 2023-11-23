using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for LoggingControl.xaml
/// </summary>
public partial class GuildControl
{
    public GuildControl()
    {
        var guildViewModel = App.ServiceProvider.GetRequiredService<GuildViewModel>();
        DataContext = guildViewModel;
        InitializeComponent();
    }
}