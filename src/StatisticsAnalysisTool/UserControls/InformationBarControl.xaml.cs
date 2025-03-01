using StatisticsAnalysisTool.ViewModels;
using System.Windows;

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
        get => (string)GetValue(InformationTextProperty);
        set => SetValue(InformationTextProperty, value);
    }

    public static readonly DependencyProperty InformationTextProperty = DependencyProperty.Register("InformationText", typeof(string), typeof(InformationBarControl));

    private void BtnInformationBar_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var mainWindowViewModel = (MainWindowViewModel) DataContext;
            
            mainWindowViewModel.InformationBarText = string.Empty;
            mainWindowViewModel.InformationBarVisibility = Visibility.Collapsed;
        });
    }
}