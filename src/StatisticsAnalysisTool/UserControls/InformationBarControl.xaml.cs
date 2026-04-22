using StatisticsAnalysisTool.HintBar;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for InformationBarControl.xaml
/// </summary>
public partial class InformationBarControl
{
    public InformationBarControl()
    {
        InitializeComponent();
    }

    public string InformationText
    {
        get => (string) GetValue(InformationTextProperty);
        set => SetValue(InformationTextProperty, value);
    }

    public static readonly DependencyProperty InformationTextProperty = DependencyProperty.Register("InformationText", typeof(string), typeof(InformationBarControl));

    private void BtnInformationBar_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (DataContext is MainWindowViewModel mainWindowViewModel)
            {
                mainWindowViewModel.InformationBarText = string.Empty;
                mainWindowViewModel.InformationBarVisibility = Visibility.Collapsed;
            }
        });
    }

    private void CopyToClipboard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        HintBarClipboard.Copy("Information", InformationText);
    }
}