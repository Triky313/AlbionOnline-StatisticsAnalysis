using CommunityToolkit.Mvvm.ComponentModel;
using StatisticsAnalysisTool.Avalonia.ViewModels.Interfaces;
using System;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class MainWindowHeaderViewModel : ViewModelBase, IMainWindowHeaderViewModel
{
    public MainWindowHeaderViewModel()
    {
        Init();
    }

    private void Init()
    {
#if DEBUG
        IsDebugMode = true;
#endif

        // Unsupported OS
        IsUnsupportedOs = Environment.OSVersion.Version.Major < 10;
    }

    [ObservableProperty]
    private bool _isDebugMode;

    [ObservableProperty]
    private bool _isUnsupportedOs;

    [ObservableProperty]
    private string _serverTypeText = "SERVER_TYPE";
}