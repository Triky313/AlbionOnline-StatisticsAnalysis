using StatisticsAnalysisTool.ViewModels;
using System.Diagnostics;
using System.Windows.Navigation;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for FooterControl.xaml
/// </summary>
public partial class FooterControl
{
    public FooterControl()
    {
        InitializeComponent();
        var footerViewModel = new FooterViewModel();
        DataContext = footerViewModel;
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
    }
}