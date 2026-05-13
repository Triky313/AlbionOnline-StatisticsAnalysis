using StatisticsAnalysisTool.ViewModels;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Navigation;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for FooterControl.xaml
/// </summary>
public partial class FooterControl
{
    private readonly FooterViewModel _footerViewModel;

    public FooterControl()
    {
        InitializeComponent();
        _footerViewModel = new FooterViewModel();
        DataContext = _footerViewModel;
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true
        });
    }

    private async void VersionLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_footerViewModel.IsUpdateAvailable)
        {
            return;
        }

        e.Handled = true;
        await _footerViewModel.OpenUpdateWindowAsync();
    }
}