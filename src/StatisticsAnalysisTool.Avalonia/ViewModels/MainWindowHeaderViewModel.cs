using CommunityToolkit.Mvvm.ComponentModel;
using StatisticsAnalysisTool.Avalonia.ViewModels.Interfaces;
using System;
using CommunityToolkit.Mvvm.Input;
using StatisticsAnalysisTool.Avalonia.Network.Enums;

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

    [RelayCommand]
    public void UpdateTrackingStatusType(TrackingStatusType trackingStatusType)
    {
        TrackingStatusType = trackingStatusType;
    }

    [ObservableProperty]
    private bool _isDebugMode;

    [ObservableProperty]
    private bool _isUnsupportedOs;

    [ObservableProperty]
    private string _serverTypeText = "SERVER__TYPE";

    [ObservableProperty]
    private string _trackingActiveText = "TRACKING__ACTIVE__TEXT";

    [ObservableProperty]
    private bool _isLoadingIconVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTrackingStatusOn))]
    [NotifyPropertyChangedFor(nameof(IsTrackingStatusOff))]
    [NotifyPropertyChangedFor(nameof(IsTrackingStatusPartially))]
    private TrackingStatusType _trackingStatusType = TrackingStatusType.Off;
    
    public bool IsTrackingStatusOn => TrackingStatusType == TrackingStatusType.On;
    public bool IsTrackingStatusOff => TrackingStatusType == TrackingStatusType.Off;
    public bool IsTrackingStatusPartially => TrackingStatusType == TrackingStatusType.Partially;
}