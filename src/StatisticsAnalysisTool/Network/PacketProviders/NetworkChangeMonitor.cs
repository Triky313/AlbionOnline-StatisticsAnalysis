#nullable enable

using Serilog;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace StatisticsAnalysisTool.Network.PacketProviders;

internal sealed class NetworkChangeMonitor : IDisposable
{
    private static readonly TimeSpan DefaultDebounceDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan DefaultFallbackInterval = TimeSpan.FromMinutes(1);

    private readonly Action _networkChanged;
    private readonly Func<string> _snapshotFactory;
    private readonly TimeSpan _debounceDelay;
    private readonly TimeSpan _fallbackInterval;
    private readonly Lock _lock = new();
    private Timer? _debounceTimer;
    private Timer? _fallbackTimer;
    private string _lastSnapshot = string.Empty;
    private bool _isRunning;

    public NetworkChangeMonitor(Action networkChanged) : this(networkChanged, CreateNetworkSnapshot, DefaultDebounceDelay, DefaultFallbackInterval) { }

    internal NetworkChangeMonitor(Action networkChanged, Func<string> snapshotFactory, TimeSpan debounceDelay, TimeSpan fallbackInterval)
    {
        _networkChanged = networkChanged ?? throw new ArgumentNullException(nameof(networkChanged));
        _snapshotFactory = snapshotFactory ?? throw new ArgumentNullException(nameof(snapshotFactory));
        _debounceDelay = debounceDelay;
        _fallbackInterval = fallbackInterval;
    }

    public void Start()
    {
        lock (_lock)
        {
            if (_isRunning)
            {
                return;
            }

            _lastSnapshot = GetSnapshotSafely();
            _isRunning = true;
            _debounceTimer = new Timer(_ => CheckForChanges(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _fallbackTimer = new Timer(_ => CheckForChanges(), null, _fallbackInterval, _fallbackInterval);

            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            NetworkChange.NetworkAddressChanged -= OnNetworkAddressChanged;
            NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;

            _debounceTimer?.Dispose();
            _debounceTimer = null;
            _fallbackTimer?.Dispose();
            _fallbackTimer = null;
        }
    }

    internal bool CheckForChanges()
    {
        var currentSnapshot = GetSnapshotSafely();

        lock (_lock)
        {
            if (!_isRunning || string.Equals(_lastSnapshot, currentSnapshot, StringComparison.Ordinal))
            {
                return false;
            }

            _lastSnapshot = currentSnapshot;
        }

        try
        {
            _networkChanged();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Network change callback failed");
        }

        return true;
    }

    public void Dispose()
    {
        Stop();
    }

    private void OnNetworkAddressChanged(object? sender, EventArgs e)
    {
        ScheduleCheck();
    }

    private void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
    {
        ScheduleCheck();
    }

    private void ScheduleCheck()
    {
        lock (_lock)
        {
            if (!_isRunning)
            {
                return;
            }

            _debounceTimer?.Change(_debounceDelay, Timeout.InfiniteTimeSpan);
        }
    }

    private string GetSnapshotSafely()
    {
        try
        {
            return _snapshotFactory();
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Network adapter snapshot could not be created");
            return string.Empty;
        }
    }

    private static string CreateNetworkSnapshot()
    {
        return string.Join("|", NetworkInterface.GetAllNetworkInterfaces()
            .OrderBy(networkInterface => networkInterface.Id, StringComparer.Ordinal)
            .Select(CreateNetworkInterfaceSnapshot));
    }

    private static string CreateNetworkInterfaceSnapshot(NetworkInterface networkInterface)
    {
        try
        {
            var properties = networkInterface.GetIPProperties();
            var unicastAddresses = string.Join(",", properties.UnicastAddresses
                .Select(address => $"{address.Address}/{address.PrefixLength}")
                .OrderBy(address => address, StringComparer.Ordinal));
            var gateways = string.Join(",", properties.GatewayAddresses
                .Select(gateway => gateway.Address.ToString())
                .OrderBy(address => address, StringComparer.Ordinal));

            return $"{networkInterface.Id};{networkInterface.NetworkInterfaceType};{networkInterface.OperationalStatus};{unicastAddresses};{gateways}";
        }
        catch (NetworkInformationException)
        {
            return $"{networkInterface.Id};{networkInterface.NetworkInterfaceType};{networkInterface.OperationalStatus}";
        }
    }
}