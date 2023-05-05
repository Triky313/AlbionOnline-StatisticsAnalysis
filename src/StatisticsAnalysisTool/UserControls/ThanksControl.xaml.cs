using StatisticsAnalysisTool.ViewModels;
using System.Windows.Controls;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for ThanksControl.xaml
/// </summary>
public partial class ThanksControl
{
    public ThanksControl()
    {
        InitializeComponent();
        DataContext = new ThanksViewModel();
    }
}