using StatisticsAnalysisTool.HintBar;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for WarningBarControl.xaml
/// </summary>
public partial class WarningBarControl
{
    public WarningBarControl()
    {
        InitializeComponent();
    }

    public string WarningText
    {
        get => (string) GetValue(WarningTextProperty);
        set => SetValue(WarningTextProperty, value);
    }

    public static readonly DependencyProperty WarningTextProperty = DependencyProperty.Register("WarningText", typeof(string), typeof(WarningBarControl));

    private void BtnWarningBar_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (DataContext is MainWindowViewModel mainWindowViewModel)
            {
                mainWindowViewModel.WarningBarText = string.Empty;
                mainWindowViewModel.WarningBarVisibility = Visibility.Collapsed;
            }
        });
    }

    private void CopyToClipboard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        HintBarClipboard.Copy("Warning", WarningText);
    }
}