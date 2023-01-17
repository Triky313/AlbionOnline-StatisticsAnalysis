using StatisticsAnalysisTool.ViewModels;
using System.Windows;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for ErrorBarControl.xaml
/// </summary>
public partial class ErrorBarControl
{
    public ErrorBarControl()
    {
        InitializeComponent();
    }
        
    public string ErrorBarText
    {
        get => (string)GetValue(ErrorBarTextProperty);
        set => SetValue(ErrorBarTextProperty, value);
    }

    public static readonly DependencyProperty ErrorBarTextProperty = DependencyProperty.Register("ErrorBarText", typeof(string), typeof(ErrorBarControl));

    private void BtnErrorBar_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = (MainWindowViewModel)DataContext;

        mainWindowViewModel.ErrorBarText = string.Empty;
        mainWindowViewModel.ErrorBarVisibility = Visibility.Collapsed;
    }
}