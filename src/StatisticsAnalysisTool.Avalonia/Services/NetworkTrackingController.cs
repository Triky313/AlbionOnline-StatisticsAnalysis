using Serilog;
using StatisticsAnalysisTool.Avalonia.Handlers;
using StatisticsAnalysisTool.Avalonia.Models;
using StatisticsAnalysisTool.Network;
using StatisticsAnalysisTool.Network.PacketProviders;
using System;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Avalonia.Services;

[SupportedOSPlatform("windows")]
public sealed class NetworkTrackingController : INetworkTrackingController
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

    private PacketProvider? _packetProvider;
    private PacketActivityMonitor? _packetActivityMonitor;
    private TrackingLifecycleStatus _status = TrackingLifecycleStatus.Stopped;
    private string _activeAdapter = string.Empty;
    private string _lastError = string.Empty;
    private PacketActivitySnapshot _packetActivity = PacketActivitySnapshot.Empty;
    private bool _isDisposed;

    public event EventHandler<TrackingStateChangedEventArgs>? StateChanged;

    public async Task StartAsync(NetworkCaptureOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            if (_packetProvider?.IsRunning == true)
            {
                PublishState(TrackingLifecycleStatus.Running, _activeAdapter, string.Empty, _packetActivity);
                return;
            }

            PublishState(TrackingLifecycleStatus.Starting, string.Empty, string.Empty, PacketActivitySnapshot.Empty);

            ReceiverBuilder receiverBuilder = ReceiverBuilder.Create();
            PacketActivityMonitor packetActivityMonitor = new();
            packetActivityMonitor.SnapshotChanged += OnPacketActivitySnapshotChanged;

            receiverBuilder.AddHandler(new EventPacketObserverHandler(packetActivityMonitor));
            receiverBuilder.AddHandler(new RequestPacketObserverHandler(packetActivityMonitor));
            receiverBuilder.AddHandler(new ResponsePacketObserverHandler(packetActivityMonitor));

            PacketProvider packetProvider = PacketProviderFactory.Create(receiverBuilder.Build(), options);
            packetProvider.ActiveAdapterChanged += OnActiveAdapterChanged;

            _packetProvider = packetProvider;
            _packetActivityMonitor = packetActivityMonitor;

            packetProvider.Start();

            Log.Information("Tracking started with provider {PacketProvider}", options.PacketProvider);
            PublishState(TrackingLifecycleStatus.Running, packetProvider.ActiveAdapterName, string.Empty, packetActivityMonitor.Snapshot);
        }
        catch (Exception ex)
        {
            CleanupPacketProvider();

            string userMessage = GetUserFacingErrorMessage(ex);
            Log.Error(ex, "Tracking start failed");
            PublishState(TrackingLifecycleStatus.Error, string.Empty, userMessage, PacketActivitySnapshot.Empty);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task StopAsync()
    {
        await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            if (_packetProvider is null)
            {
                PublishState(TrackingLifecycleStatus.Stopped, string.Empty, string.Empty, _packetActivity);
                return;
            }

            PublishState(TrackingLifecycleStatus.Stopping, _activeAdapter, string.Empty, _packetActivity);

            _packetProvider.Stop();
            Log.Information("Tracking stopped");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Tracking stop failed");
            PublishState(TrackingLifecycleStatus.Error, _activeAdapter, "Tracking could not be stopped cleanly. Please restart the application.", _packetActivity);
        }
        finally
        {
            CleanupPacketProvider();
            PublishState(TrackingLifecycleStatus.Stopped, string.Empty, string.Empty, _packetActivity);
            _semaphoreSlim.Release();
        }
    }

    public async Task RestartAsync(NetworkCaptureOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        await StopAsync().ConfigureAwait(false);
        await StartAsync(options).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        try
        {
            StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch
        {
        }

        _isDisposed = true;
        _semaphoreSlim.Dispose();
    }

    private void OnPacketActivitySnapshotChanged(object? sender, PacketActivitySnapshot snapshot)
    {
        PublishState(_status, _activeAdapter, _lastError, snapshot);
    }

    private void OnActiveAdapterChanged(object? sender, string? activeAdapterName)
    {
        PublishState(_status, activeAdapterName ?? string.Empty, _lastError, _packetActivity);
    }

    private void PublishState(TrackingLifecycleStatus status, string activeAdapter, string lastError, PacketActivitySnapshot packetActivity)
    {
        _status = status;
        _activeAdapter = activeAdapter;
        _lastError = lastError;
        _packetActivity = packetActivity;

        StateChanged?.Invoke(this, new TrackingStateChangedEventArgs
        {
            ActiveAdapter = _activeAdapter,
            LastError = _lastError,
            PacketActivity = _packetActivity,
            Status = _status
        });
    }

    private void CleanupPacketProvider()
    {
        if (_packetProvider is not null)
        {
            try
            {
                if (_packetProvider.IsRunning)
                {
                    _packetProvider.Stop();
                }
            }
            catch
            {
            }

            _packetProvider.ActiveAdapterChanged -= OnActiveAdapterChanged;
        }

        if (_packetActivityMonitor is not null)
        {
            _packetActivityMonitor.SnapshotChanged -= OnPacketActivitySnapshotChanged;
        }

        _packetProvider = null;
        _packetActivityMonitor = null;
    }

    private static string GetUserFacingErrorMessage(Exception exception)
    {
        if (exception is NetworkCaptureException networkCaptureException && !string.IsNullOrWhiteSpace(networkCaptureException.Message))
        {
            return networkCaptureException.Message;
        }

        if (exception is SocketException socketException)
        {
            return $"Socket capture could not start (code: {(int)socketException.SocketErrorCode}). Run the app as Administrator or switch to Npcap.";
        }

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

        if (exception is UnauthorizedAccessException)
        {
            return "Tracking requires elevated permissions for the current packet provider.";
        }

        return "Tracking could not be started. Check the packet provider and network adapter configuration.";
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

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(NetworkTrackingController));
        }
    }
}
