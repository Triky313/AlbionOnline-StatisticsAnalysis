using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for LoggingControl.xaml
/// </summary>
public partial class GuildControl
{
    private readonly GuildViewModel _guildViewModel;

    public GuildControl()
    {
        InitializeComponent();
        _guildViewModel = new GuildViewModel();
        DataContext = _guildViewModel;
    }

    private void BtnDeleteSelectedSiphonedEnergyEntries_Click(object sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void OpenSiphonedEnergyInfoPopup_MouseEnter(object sender, MouseEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void CloseSiphonedEnergyInfoPopup_MouseLeave(object sender, MouseEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void BtnSelectSwitchAllSiphonedEnergyEntries_Click(object sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }
}