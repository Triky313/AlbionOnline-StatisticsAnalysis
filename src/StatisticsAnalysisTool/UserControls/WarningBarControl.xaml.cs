using StatisticsAnalysisTool.ViewModels;
using System.Windows;

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
        get => (string)GetValue(WarningTextProperty);
        set => SetValue(WarningTextProperty, value);
    }

    public static readonly DependencyProperty WarningTextProperty = DependencyProperty.Register("WarningText", typeof(string), typeof(WarningBarControl));

    private void BtnWarningBar_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var mainWindowViewModel = (MainWindowViewModel) DataContext;

            mainWindowViewModel.WarningBarText = string.Empty;
            mainWindowViewModel.WarningBarVisibility = Visibility.Collapsed;
        });
    }
}