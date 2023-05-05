using StatisticsAnalysisTool.ViewModels;
using System.Diagnostics;
using System.Windows.Navigation;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for DonationControl.xaml
/// </summary>
public partial class DonationControl
{
    public DonationControl()
    {
        InitializeComponent();
        DataContext = new DonationViewModel();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
    }
}