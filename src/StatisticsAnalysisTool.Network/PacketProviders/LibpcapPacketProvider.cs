#nullable enable

using BinaryFormat;
using BinaryFormat.EthernetFrame;
using BinaryFormat.IPv4;
using BinaryFormat.Udp;
using Libpcap;
using Serilog;
using StatisticsAnalysisTool.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace StatisticsAnalysisTool.Network.PacketProviders;

public sealed class LibpcapPacketProvider : PacketProvider
{
    private readonly IPhotonReceiver _photonReceiver;
    private readonly NetworkCaptureOptions _options;
    private readonly Lock _lockObj = new();
    private readonly Dictionary<Pcap, int> _pcapScores = new();

    private PcapDispatcher? _dispatcher;
    private CancellationTokenSource? _cts;
    private Thread? _thread;
    private volatile Pcap? _activePcap;
    private DateTime _lastValidPacketUtc = DateTime.MinValue;

    private const int ScoreToLock = 1;
    private static readonly TimeSpan LockIdleTimeout = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan DispatchErrorBackoff = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan StopThreadJoinTimeout = TimeSpan.FromSeconds(2);
    private const int ConsecutiveDispatchErrorsBeforeEscalation = 20;

    public override bool IsRunning => _thread is { IsAlive: true };

    public LibpcapPacketProvider(IPhotonReceiver photonReceiver, NetworkCaptureOptions options)
    {
        _photonReceiver = photonReceiver ?? throw new ArgumentNullException(nameof(photonReceiver));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _dispatcher = new PcapDispatcher(Dispatch);
    }

    public override void Start()
    {
        if (_thread is { IsAlive: true })
        {
            return;
        }

        _activePcap = null;
        _pcapScores.Clear();
        UpdateActiveAdapterName(null);

        _dispatcher?.Dispose();
        _dispatcher = new PcapDispatcher(Dispatch);

        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        PcapDispatcher? dispatcher = _dispatcher;
        if (dispatcher is null)
        {
            throw new NetworkCaptureException("Npcap dispatcher is unavailable.");
        }

        var devices = Pcap.ListDevices();
        if (devices.Count == 0)
        {
            throw new NetworkCaptureException("No network adapters were found for Npcap capture.");
        }

        string? filter = GetEffectiveFilter();
        bool hasFilter = !string.IsNullOrWhiteSpace(filter);

        int configuredIndex = _options.LegacyNetworkDeviceIndex;
        IReadOnlyList<ConfiguredNetworkDevice> configuredDevices = _options.NetworkDevices;
        bool hasConfiguredDevices = configuredDevices.Count > 0;
        HashSet<string> selectedDeviceIdentifiers = configuredDevices
            .Where(x => x.IsSelected && !string.IsNullOrWhiteSpace(x.Identifier))
            .Select(x => x.Identifier)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        int opened = 0;
        Exception? lastOpenException = null;

        for (int i = 0; i < devices.Count; i++)
        {
            PcapDevice device = devices[i];

            if (!hasConfiguredDevices && configuredIndex >= 0 && i != configuredIndex)
            {
                continue;
            }

            if (device.Flags.HasFlag(PcapDeviceFlags.Loopback))
            {
                Log.Information("Npcap[ID:{Index}]: skip loopback {Name}:{Desc}", i, device.Name, device.Description);
                continue;
            }

            if (!device.Flags.HasFlag(PcapDeviceFlags.Up))
            {
                Log.Information("Npcap[ID:{Index}]: skip down {Name}:{Desc}", i, device.Name, device.Description);
                continue;
            }

            if (hasConfiguredDevices && !selectedDeviceIdentifiers.Contains(device.Name))
            {
                Log.Information("Npcap[ID:{Index}]: skip disabled {Name}:{Desc}", i, device.Name, device.Description);
                continue;
            }

            try
            {
                Log.Information("Npcap[ID:{Index}]: opening {Name}:{Desc} (Type={Type}, Flags={Flags})",
                    i,
                    device.Name,
                    device.Description,
                    device.Type,
                    device.Flags);

                dispatcher.OpenDevice(device, pcap =>
                {
                    pcap.NonBlocking = true;
                });

                if (hasFilter)
                {
                    dispatcher.Filter = filter!;
                    Log.Information("Npcap[ID:{Index}]: filter set => {Filter}", i, filter);
                }
                else
                {
                    Log.Information("Npcap[ID:{Index}]: no filter (capturing all)", i);
                }

                opened++;

                if (!hasConfiguredDevices && configuredIndex >= 0)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                lastOpenException = ex;
                Log.Error(ex, "Npcap[ID:{Index}]: open failed for {Name}:{Desc}", i, device.Name, device.Description);
            }
        }

        if (opened == 0)
        {
            throw lastOpenException is null
                ? new NetworkCaptureException("Npcap could not open a network adapter. Check administrator rights, firewall/VPN, and the selected adapter.")
                : new NetworkCaptureException("Npcap could not open a network adapter. Check administrator rights, firewall/VPN, and the selected adapter.", lastOpenException);
        }

        _thread = new Thread(Worker)
        {
            IsBackground = true
        };
        _thread.Start();

        Log.Information("Npcap: capture started on {Opened} device(s), filter: {Filter}", opened, hasFilter ? filter : "<none>");
    }

    private void Dispatch(Pcap pcap, ref Packet packet)
    {
        Pcap? current = _activePcap;
        if (current is not null && !ReferenceEquals(current, pcap))
        {
            return;
        }

        BinaryFormatReader ethReader = new(packet.Data);
        L2EthernetFrameShape eth = new();
        if (!ethReader.TryReadL2EthernetFrame(ref eth))
        {
            return;
        }

        ushort etherType = (ushort)((packet.Data[12] << 8) | packet.Data[13]);
        ReadOnlySpan<byte> l3 = eth.Payload;

        if (etherType == 0x0800)
        {
            if (IsFragmentedIPv4(l3))
            {
                return;
            }

            BinaryFormatReader ipReader = new(l3);
            IPv4PacketShape ip4 = new();
            if (!ipReader.TryReadIPv4Packet(ref ip4))
            {
                return;
            }

            switch ((ProtocolType)ip4.Protocol)
            {
                case ProtocolType.Udp:
                    HandleUdp(ip4.Payload, pcap);
                    return;
                case ProtocolType.Tcp:
                    return;
                default:
                    return;
            }
        }

        if (etherType == 0x86DD)
        {
            if (!TryReadIPv6(l3, out byte nextHeader, out ReadOnlySpan<byte> ip6Payload))
            {
                return;
            }

            switch ((ProtocolType)nextHeader)
            {
                case ProtocolType.Udp:
                    HandleUdp(ip6Payload, pcap);
                    return;
                case ProtocolType.Tcp:
                    return;
                default:
                    return;
            }
        }
    }

    private static bool IsFragmentedIPv4(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 8)
        {
            return true;
        }

        ushort flagsAndFragmentOffset = (ushort)((bytes[6] << 8) | bytes[7]);
        bool hasMoreFragments = (flagsAndFragmentOffset & 0x2000) != 0;
        int fragmentOffset = (flagsAndFragmentOffset & 0x1FFF) * 8;

        return hasMoreFragments || fragmentOffset != 0;
    }

    private static bool TryReadIPv6(ReadOnlySpan<byte> bytes, out byte nextHeader, out ReadOnlySpan<byte> payload)
    {
        nextHeader = 0;
        payload = default;

        if (bytes.Length < 40)
        {
            return false;
        }

        nextHeader = bytes[6];
        payload = bytes[40..];
        return true;
    }

    private void HandleUdp(ReadOnlySpan<byte> l4Payload, Pcap pcap)
    {
        BinaryFormatReader udpReader = new(l4Payload);
        UdpPacketShape udp = new();
        if (!udpReader.TryReadUdpPacket(ref udp))
        {
            return;
        }

        bool isPhotonPort = PhotonPorts.Udp.Contains(udp.SourcePort) || PhotonPorts.Udp.Contains(udp.DestinationPort);
        bool looksPhoton = isPhotonPort || LooksLikePhoton(udp.Payload);
        if (!looksPhoton || udp.Payload.Length == 0)
        {
            return;
        }

        SelectAndMaybeLockAdapter(pcap);

        Pcap? current = _activePcap;
        if (current is not null && !ReferenceEquals(current, pcap))
        {
            return;
        }

        _lastValidPacketUtc = DateTime.UtcNow;

        try
        {
            _photonReceiver.ReceivePacket(udp.Payload);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "PhotonReceiver.ReceivePacket failed");
        }
    }

    private void SelectAndMaybeLockAdapter(Pcap pcap)
    {
        lock (_lockObj)
        {
            if (_activePcap is not null)
            {
                if (DateTime.UtcNow - _lastValidPacketUtc > LockIdleTimeout)
                {
                    Log.Information("Npcap: releasing locked adapter due to inactivity");
                    _activePcap = null;
                    _pcapScores.Clear();
                    UpdateActiveAdapterName(null);
                }
                else
                {
                    return;
                }
            }

            int score = _pcapScores.GetValueOrDefault(pcap, 0);
            score++;
            _pcapScores[pcap] = score;

            if (score >= ScoreToLock)
            {
                _activePcap = pcap;
                _lastValidPacketUtc = DateTime.UtcNow;
                UpdateActiveAdapterName(pcap.Name);
                Log.Information("Npcap: locked to adapter({device}) after {Score} valid packets", pcap.Name, score);
            }
        }
    }

    private static bool LooksLikePhoton(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < 3)
        {
            return false;
        }

        byte b0 = payload[0];
        return b0 is 0xF1 or 0xF2 or 0xFE;
    }

    private void Worker()
    {
        try
        {
            PcapDispatcher? dispatcher = _dispatcher;
            if (dispatcher is null)
            {
                return;
            }

            int consecutiveDispatchErrors = 0;

            while (_cts is { IsCancellationRequested: false })
            {
                int dispatched;
                try
                {
                    dispatched = dispatcher.Dispatch(50);
                    consecutiveDispatchErrors = 0;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (InvalidOperationException)
                {
                    break;
                }
                catch (PcapException ex)
                {
                    consecutiveDispatchErrors++;
                    LogDispatchError(ex, consecutiveDispatchErrors);
                    _cts?.Token.WaitHandle.WaitOne(DispatchErrorBackoff);
                    continue;
                }

                if (dispatched <= 0)
                {
                    _cts?.Token.WaitHandle.WaitOne(25);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Libpcap worker crashed");
        }
    }

    private static void LogDispatchError(PcapException ex, int consecutiveDispatchErrors)
    {
        if (consecutiveDispatchErrors == ConsecutiveDispatchErrorsBeforeEscalation)
        {
            Log.Error(ex, "Libpcap: pcap_dispatch failing repeatedly ({Count}x); capture may be degraded", consecutiveDispatchErrors);
            return;
        }

        if (consecutiveDispatchErrors < ConsecutiveDispatchErrorsBeforeEscalation)
        {
            Log.Warning(ex, "Libpcap: pcap_dispatch failed, retrying (attempt {Count})", consecutiveDispatchErrors);
        }
    }

    public override void Stop()
    {
        try
        {
            _cts?.Cancel();
            _dispatcher?.Dispose();

            if (_thread is { IsAlive: true } && !_thread.Join(StopThreadJoinTimeout))
            {
                Log.Warning("Npcap: worker did not stop within {TimeoutMs} ms", StopThreadJoinTimeout.TotalMilliseconds);
            }
        }
        finally
        {
            _activePcap = null;
            _pcapScores.Clear();
            _lastValidPacketUtc = DateTime.MinValue;
            UpdateActiveAdapterName(null);

            _cts?.Dispose();
            _cts = null;
            _thread = null;
            _dispatcher = null;
        }
    }

    public static class PhotonPorts
    {
        public static readonly HashSet<ushort> Udp = [5055, 5056, 5058];
        public static readonly HashSet<ushort> Tcp = [4530, 4531, 4533];
    }

    public static IReadOnlyList<NetworkDeviceInformation> GetAvailableNetworkDevices()
    {
        List<NetworkDeviceInformation> result = [];
        var devices = Pcap.ListDevices();

        for (int i = 0; i < devices.Count; i++)
        {
            PcapDevice device = devices[i];

            if (device.Flags.HasFlag(PcapDeviceFlags.Loopback))
            {
                continue;
            }

            if (!device.Flags.HasFlag(PcapDeviceFlags.Up))
            {
                continue;
            }

            string name = string.IsNullOrWhiteSpace(device.Description)
                ? device.Name
                : $"{device.Description} ({device.Name})";

            result.Add(new NetworkDeviceInformation
            {
                Identifier = device.Name,
                Name = name,
                Index = i
            });
        }

        return result;
    }

    private string? GetEffectiveFilter()
    {
        return string.IsNullOrWhiteSpace(_options.PacketFilter) ? null : _options.PacketFilter;
    }
}
