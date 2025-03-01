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
        
    public string ErrorText
    {
        get => (string)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    public static readonly DependencyProperty ErrorTextProperty = DependencyProperty.Register("ErrorText", typeof(string), typeof(ErrorBarControl));

    private void BtnErrorBar_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var mainWindowViewModel = (MainWindowViewModel) DataContext;

            mainWindowViewModel.ErrorBarText = string.Empty;
            mainWindowViewModel.ErrorBarVisibility = Visibility.Collapsed;
        });
    }
}