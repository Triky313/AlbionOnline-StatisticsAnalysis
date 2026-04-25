using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using StatisticsAnalysisTool.Avalonia.Models;
using StatisticsAnalysisTool.Avalonia.Services;
using StatisticsAnalysisTool.Network.PacketProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const int MaxLogLines = 250;

    private readonly INetworkTrackingController? _networkTrackingController;
    private readonly TrackingSettingsStore? _trackingSettingsStore;
    private readonly UiLogSink? _uiLogSink;

    private TrackingSettingsDocument _settingsDocument = new();

    public MainWindowViewModel()
    {
    }

    public MainWindowViewModel(
        INetworkTrackingController networkTrackingController,
        TrackingSettingsStore trackingSettingsStore,
        UiLogSink uiLogSink)
    {
        _networkTrackingController = networkTrackingController ?? throw new ArgumentNullException(nameof(networkTrackingController));
        _trackingSettingsStore = trackingSettingsStore ?? throw new ArgumentNullException(nameof(trackingSettingsStore));
        _uiLogSink = uiLogSink ?? throw new ArgumentNullException(nameof(uiLogSink));

        _networkTrackingController.StateChanged += OnTrackingStateChanged;
        _uiLogSink.LogReceived += OnLogReceived;

        _ = InitializeAsync();
    }

    public ObservableCollection<NetworkAdapterItemViewModel> AvailableAdapters { get; } = [];

    public ObservableCollection<string> LogLines { get; } = [];

    public IReadOnlyList<PacketProviderKind> PacketProviders { get; } =
    [
        PacketProviderKind.Npcap,
        PacketProviderKind.Sockets
    ];

    [ObservableProperty]
    private PacketProviderKind _selectedPacketProvider = PacketProviderKind.Npcap;

    [ObservableProperty]
    private string _trackingStatus = TrackingLifecycleStatus.Stopped.ToString();

    [ObservableProperty]
    private string _selectedAdapter = "All eligible adapters";

    [ObservableProperty]
    private string _activeAdapter = "Not running";

    [ObservableProperty]
    private string _lastError = string.Empty;

    [ObservableProperty]
    private string _lastPacket = PacketActivitySnapshot.Empty.LastPacket;

    [ObservableProperty]
    private long _totalPackets;

    [ObservableProperty]
    private long _eventPackets;

    [ObservableProperty]
    private long _requestPackets;

    [ObservableProperty]
    private long _responsePackets;

    [ObservableProperty]
    private bool _isTrackingRunning;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _settingsFilePath = string.Empty;

    public bool IsNpcapProviderSelected => SelectedPacketProvider == PacketProviderKind.Npcap;

    public bool CanStartTracking => !IsBusy && !IsTrackingRunning;

    public bool CanStopTracking => !IsBusy && IsTrackingRunning;

    public bool CanRestartTracking => !IsBusy;

    public string PacketSummary => $"Total: {TotalPackets} | Events: {EventPackets} | Requests: {RequestPackets} | Responses: {ResponsePackets}";

    [RelayCommand]
    private async Task StartTrackingAsync()
    {
        if (_networkTrackingController is null || _trackingSettingsStore is null)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await PersistSettingsAsync().ConfigureAwait(false);
            await _networkTrackingController.StartAsync(BuildCaptureOptions()).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task StopTrackingAsync()
    {
        if (_networkTrackingController is null)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await _networkTrackingController.StopAsync().ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RestartTrackingAsync()
    {
        if (_networkTrackingController is null || _trackingSettingsStore is null)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await PersistSettingsAsync().ConfigureAwait(false);
            await _networkTrackingController.RestartAsync(BuildCaptureOptions()).ConfigureAwait(false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedPacketProviderChanged(PacketProviderKind value)
    {
        OnPropertyChanged(nameof(IsNpcapProviderSelected));
        UpdateSelectedAdapterSummary();

        if (AvailableAdapters.Count == 0)
        {
            LoadAvailableAdapters();
        }
    }

    partial void OnTotalPacketsChanged(long value)
    {
        OnPropertyChanged(nameof(PacketSummary));
    }

    partial void OnEventPacketsChanged(long value)
    {
        OnPropertyChanged(nameof(PacketSummary));
    }

    partial void OnRequestPacketsChanged(long value)
    {
        OnPropertyChanged(nameof(PacketSummary));
    }

    partial void OnResponsePacketsChanged(long value)
    {
        OnPropertyChanged(nameof(PacketSummary));
    }

    partial void OnIsTrackingRunningChanged(bool value)
    {
        OnPropertyChanged(nameof(CanStartTracking));
        OnPropertyChanged(nameof(CanStopTracking));
        OnPropertyChanged(nameof(CanRestartTracking));
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(CanStartTracking));
        OnPropertyChanged(nameof(CanStopTracking));
        OnPropertyChanged(nameof(CanRestartTracking));
    }

    private async Task InitializeAsync()
    {
        if (_trackingSettingsStore is null)
        {
            return;
        }

        SettingsFilePath = _trackingSettingsStore.SettingsFilePath;
        _settingsDocument = await _trackingSettingsStore.LoadAsync().ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            SelectedPacketProvider = _settingsDocument.PacketProvider;
            LoadAvailableAdapters();
            UpdateSelectedAdapterSummary();
        });

        Log.Information("Tracking settings loaded from {SettingsFilePath}", SettingsFilePath);
    }

    private void LoadAvailableAdapters()
    {
        DetachAdapterSelectionChangedHandlers();
        AvailableAdapters.Clear();

        try
        {
            IReadOnlyList<NetworkDeviceInformation> availableDevices = LibpcapPacketProvider.GetAvailableNetworkDevices();
            Dictionary<string, TrackingNetworkDeviceSettings> configuredByIdentifier = (_settingsDocument.NetworkDevices ?? [])
                .Where(x => !string.IsNullOrWhiteSpace(x.Identifier))
                .GroupBy(x => x.Identifier, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

            bool hasConfiguredDevices = configuredByIdentifier.Count > 0;
            int legacyNetworkDeviceIndex = _settingsDocument.NetworkDevice;

            foreach (NetworkDeviceInformation availableDevice in availableDevices)
            {
                bool isSelected;

                if (hasConfiguredDevices && configuredByIdentifier.TryGetValue(availableDevice.Identifier, out TrackingNetworkDeviceSettings? configuredDevice))
                {
                    isSelected = configuredDevice.IsSelected;
                }
                else if (!hasConfiguredDevices && legacyNetworkDeviceIndex >= 0)
                {
                    isSelected = availableDevice.Index == legacyNetworkDeviceIndex;
                }
                else
                {
                    isSelected = true;
                }

                NetworkAdapterItemViewModel adapterViewModel = new()
                {
                    Identifier = availableDevice.Identifier,
                    Index = availableDevice.Index,
                    IsSelected = isSelected,
                    Name = availableDevice.Name
                };

                adapterViewModel.PropertyChanged += OnAdapterSelectionChanged;
                AvailableAdapters.Add(adapterViewModel);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Network adapters could not be loaded from Npcap");

            if (SelectedPacketProvider == PacketProviderKind.Npcap)
            {
                LastError = GetAdapterLoadErrorMessage(ex);
            }
        }

        UpdateSelectedAdapterSummary();
    }

    private async Task PersistSettingsAsync()
    {
        if (_trackingSettingsStore is null)
        {
            return;
        }

        _settingsDocument.PacketProvider = SelectedPacketProvider;
        _settingsDocument.PacketFilter = string.IsNullOrWhiteSpace(_settingsDocument.PacketFilter)
            ? TrackingSettingsDocument.DefaultPacketFilter
            : _settingsDocument.PacketFilter;
        _settingsDocument.NetworkDevices = AvailableAdapters
            .Select(x => new TrackingNetworkDeviceSettings
            {
                Identifier = x.Identifier,
                IsSelected = x.IsSelected,
                Name = x.Name
            })
            .ToList();

        List<NetworkAdapterItemViewModel> selectedAdapters = AvailableAdapters.Where(x => x.IsSelected).ToList();
        _settingsDocument.NetworkDevice = selectedAdapters.Count == 1 ? selectedAdapters[0].Index : -1;

        await _trackingSettingsStore.SaveAsync(_settingsDocument).ConfigureAwait(false);
        Log.Information("Tracking settings saved to {SettingsFilePath}", _trackingSettingsStore.SettingsFilePath);
    }

    private NetworkCaptureOptions BuildCaptureOptions()
    {
        return new NetworkCaptureOptions
        {
            PacketProvider = SelectedPacketProvider,
            PacketFilter = string.IsNullOrWhiteSpace(_settingsDocument.PacketFilter)
                ? TrackingSettingsDocument.DefaultPacketFilter
                : _settingsDocument.PacketFilter,
            LegacyNetworkDeviceIndex = _settingsDocument.NetworkDevice,
            NetworkDevices = _settingsDocument.NetworkDevices
                .Select(x => new ConfiguredNetworkDevice
                {
                    Identifier = x.Identifier,
                    IsSelected = x.IsSelected,
                    Name = x.Name
                })
                .ToList()
        };
    }

    private void UpdateSelectedAdapterSummary()
    {
        if (!IsNpcapProviderSelected)
        {
            SelectedAdapter = "Not required for socket capture";
            return;
        }

        List<string> selectedAdapterNames = AvailableAdapters
            .Where(x => x.IsSelected)
            .Select(x => x.Name)
            .ToList();

        SelectedAdapter = selectedAdapterNames.Count switch
        {
            0 => "No adapter selected",
            1 => selectedAdapterNames[0],
            _ => $"{selectedAdapterNames.Count} adapters selected"
        };
    }

    private void OnAdapterSelectionChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (!string.Equals(eventArgs.PropertyName, nameof(NetworkAdapterItemViewModel.IsSelected), StringComparison.Ordinal))
        {
            return;
        }

        UpdateSelectedAdapterSummary();
    }

    private void OnTrackingStateChanged(object? sender, TrackingStateChangedEventArgs eventArgs)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TrackingStatus = eventArgs.Status.ToString();
            IsTrackingRunning = eventArgs.Status == TrackingLifecycleStatus.Running;
            ActiveAdapter = string.IsNullOrWhiteSpace(eventArgs.ActiveAdapter)
                ? eventArgs.Status == TrackingLifecycleStatus.Running
                    ? "Waiting for an active adapter..."
                    : "Not running"
                : eventArgs.ActiveAdapter;
            LastError = eventArgs.LastError;
            LastPacket = eventArgs.PacketActivity.LastPacket;
            TotalPackets = eventArgs.PacketActivity.TotalPackets;
            EventPackets = eventArgs.PacketActivity.EventPackets;
            RequestPackets = eventArgs.PacketActivity.RequestPackets;
            ResponsePackets = eventArgs.PacketActivity.ResponsePackets;
        });
    }

    private void OnLogReceived(object? sender, string logLine)
    {
        Dispatcher.UIThread.Post(() =>
        {
            LogLines.Insert(0, logLine);

            while (LogLines.Count > MaxLogLines)
            {
                LogLines.RemoveAt(LogLines.Count - 1);
            }
        });
    }

    private static string GetAdapterLoadErrorMessage(Exception exception)
    {
        if (exception is DllNotFoundException dllNotFoundException && ContainsNpcapReference(dllNotFoundException.Message))
        {
            return "Npcap (wpcap.dll) was not found. Please install or repair Npcap (without WinPcap legacy).";
        }

        if (exception is TypeInitializationException
            {
                InnerException: DllNotFoundException innerException
            }
            && ContainsNpcapReference(innerException.Message))
        {
            return "Npcap (wpcap.dll) was not found. Please install or repair Npcap (without WinPcap legacy).";
        }

        return "Network adapters could not be loaded. Check the Npcap installation and the selected packet provider.";
    }

    private static bool ContainsNpcapReference(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return message.Contains("wpcap", StringComparison.OrdinalIgnoreCase)
               || message.Contains("npcap", StringComparison.OrdinalIgnoreCase);
    }

    private void DetachAdapterSelectionChangedHandlers()
    {
        foreach (NetworkAdapterItemViewModel adapterViewModel in AvailableAdapters)
        {
            adapterViewModel.PropertyChanged -= OnAdapterSelectionChanged;
        }
    }
}
