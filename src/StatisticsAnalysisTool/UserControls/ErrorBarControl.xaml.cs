using StatisticsAnalysisTool.HintBar;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

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
        get => (string) GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    public static readonly DependencyProperty ErrorTextProperty = DependencyProperty.Register("ErrorText", typeof(string), typeof(ErrorBarControl));

    public Exception ErrorException
    {
        get => (Exception) GetValue(ErrorExceptionProperty);
        set => SetValue(ErrorExceptionProperty, value);
    }

    public static readonly DependencyProperty ErrorExceptionProperty = DependencyProperty.Register("ErrorException", typeof(Exception), typeof(ErrorBarControl));

    private void BtnErrorBar_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (DataContext is MainWindowViewModel mainWindowViewModel)
            {
                mainWindowViewModel.ErrorBarText = string.Empty;
                mainWindowViewModel.ErrorBarException = null;
                mainWindowViewModel.ErrorBarVisibility = Visibility.Collapsed;
                return;
            }

            if (DataContext is ItemWindowViewModel itemWindowViewModel)
            {
                itemWindowViewModel.ErrorBarText = string.Empty;
                itemWindowViewModel.ErrorBarException = null;
                itemWindowViewModel.ErrorBarVisibility = Visibility.Collapsed;
            }
        });
    }

    private void CopyToClipboard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        HintBarClipboard.Copy("Error", ErrorText, ErrorException);
    }
}