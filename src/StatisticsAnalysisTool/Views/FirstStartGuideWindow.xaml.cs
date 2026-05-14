using StatisticsAnalysisTool.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace StatisticsAnalysisTool.Views;

public partial class FirstStartGuideWindow
{
    private readonly FirstStartGuideViewModel _viewModel;

    public FirstStartGuideWindow()
    {
        InitializeComponent();
        _viewModel = new FirstStartGuideViewModel();
        _viewModel.Completed += ViewModel_Completed;
        DataContext = _viewModel;
    }

    private void ViewModel_Completed(object sender, EventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void BtnSelectMainGameFolder_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.OpenPathSelection();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true
        });
    }

    private void LanguageComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.ComboBox { SelectedItem: FirstStartGuideLanguageOption languageOption })
        {
            _viewModel.SelectLanguage(languageOption);
        }
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Back();
    }

    private async void BtnNext_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.NextAsync();
    }

    private async void BtnFinish_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.FinishAsync();
    }
}
