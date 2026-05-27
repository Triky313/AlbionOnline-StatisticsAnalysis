#nullable enable

using BinaryFormat;
using BinaryFormat.EthernetFrame;
using BinaryFormat.IPv4;
using BinaryFormat.Udp;
using Libpcap;
using Serilog;
using StatisticsAnalysisTool.Abstractions;
using StatisticsAnalysisTool.Common.UserSettings;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace StatisticsAnalysisTool.Network.PacketProviders;

public class LibpcapPacketProvider : PacketProvider
{
    private readonly IPhotonReceiver _photonReceiver;
    private readonly AlbionServerDetectionService _albionServerDetectionService;
    private PcapDispatcher? _dispatcher;
    private CancellationTokenSource? _cts;
    private Thread? _thread;
    private volatile Pcap? _activePcap;
    private readonly Lock _lockObj = new();
    private readonly Lock _dispatcherLock = new();
    private readonly Lock _ipv4FragmentsLock = new();
    private readonly Dictionary<Pcap, int> _pcapScores = new();
    private readonly Dictionary<IPv4FragmentKey, IPv4FragmentBuffer> _ipv4Fragments = new();
    private DateTime _lastValidPacketUtc = DateTime.MinValue;
    private DateTime _lastRecoveryFailureLogUtc = DateTime.MinValue;

    public const string DefaultPacketFilter = "((ip and ((udp and (port 5055 or port 5056 or port 5058)) or (ip[6:2] & 0x3fff != 0))) or (ip6 and (udp and (port 5055 or port 5056 or port 5058))))";
    private const string LegacyDefaultPacketFilter = "(ip or ip6) and (udp and (port 5055 or port 5056 or port 5058))";
    private const int MaxIPv4PayloadLength = 65535;
    private const int MaxPendingIPv4FragmentPackages = 256;
    private const int ScoreToLock = 1;
    private static readonly TimeSpan LockIdleTimeout = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan DispatchErrorBackoff = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan CaptureRecoveryBackoff = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan RecoveryFailureLogInterval = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan IPv4FragmentTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan StopThreadJoinTimeout = TimeSpan.FromSeconds(2);
    private const int ConsecutiveDispatchErrorsBeforeEscalation = 20;
    private const int ConsecutiveDispatchErrorsBeforeRecovery = ConsecutiveDispatchErrorsBeforeEscalation;

    public override bool IsRunning
    {
        get
        {
            return _thread?.IsAlive == true;
        }
    }

    public LibpcapPacketProvider(IPhotonReceiver photonReceiver, AlbionServerDetectionService albionServerDetectionService)
    {
        _photonReceiver = photonReceiver ?? throw new ArgumentNullException(nameof(photonReceiver));
        _albionServerDetectionService = albionServerDetectionService ?? throw new ArgumentNullException(nameof(albionServerDetectionService));
    }

    public override void Start()
    {
        if (_thread?.IsAlive == true)
        {
            return;
        }

        ResetAdapterSelection();

        lock (_dispatcherLock)
        {
            _dispatcher?.Dispose();
            _dispatcher = null;
        }

        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        var dispatcher = CreateDispatcher();
        int opened;

        try
        {
            opened = OpenConfiguredDevices(dispatcher, true);
        }
        catch
        {
            dispatcher.Dispose();
            throw;
        }

        if (opened == 0)
        {
            dispatcher.Dispose();
            Log.Warning("Npcap: no device opened (check NetworkDevice index or admin rights)");
            return;
        }

        lock (_dispatcherLock)
        {
            _dispatcher = dispatcher;
        }

        _thread = new Thread(Worker)
        {
            IsBackground = true
        };
        _thread.Start();

        var filter = GetEffectiveFilter();
        Log.Information("Npcap: capture started on {Opened} device(s), filter: {Filter}", opened, string.IsNullOrWhiteSpace(filter) ? "<none>" : filter);
    }

    private PcapDispatcher CreateDispatcher()
    {
        return new PcapDispatcher(Dispatch);
    }

    private int OpenConfiguredDevices(PcapDispatcher dispatcher, bool logDeviceDetails)
    {
        var devices = Pcap.ListDevices();
        if (devices.Count == 0)
        {
            if (logDeviceDetails)
            {
                Log.Warning("Npcap: no devices found");
            }

            return 0;
        }

        var filter = GetEffectiveFilter();
        bool hasFilter = !string.IsNullOrWhiteSpace(filter);

        var configuredDevices = SettingsController.CurrentSettings.NetworkDevices ?? new List<NetworkDeviceSettingsObject>();

        int opened = 0;
        for (int i = 0; i < devices.Count; i++)
        {
            var device = devices[i];
            var configuredDevice = configuredDevices
                .FirstOrDefault(x => string.Equals(x?.Identifier, device.Name, StringComparison.OrdinalIgnoreCase));

            if (configuredDevice?.IsSelected == false)
            {
                if (logDeviceDetails)
                {
                    Log.Information("Npcap[ID:{Index}]: skip disabled {Name}:{Desc}", i, device.Name, device.Description);
                }

                continue;
            }

            if (device.Flags.HasFlag(PcapDeviceFlags.Loopback))
            {
                if (logDeviceDetails)
                {
                    Log.Information("Npcap[ID:{Index}]: skip loopback {Name}:{Desc}", i, device.Name, device.Description);
                }

                continue;
            }

            if (!device.Flags.HasFlag(PcapDeviceFlags.Up))
            {
                if (logDeviceDetails)
                {
                    Log.Information("Npcap[ID:{Index}]: skip down {Name}:{Desc}", i, device.Name, device.Description);
                }

                continue;
            }

            try
            {
                if (logDeviceDetails)
                {
                    Log.Information("Npcap[ID:{Index}]: opening {Name}:{Desc} (Type={Type}, Flags={Flags})",
                        i, device.Name, device.Description, device.Type, device.Flags);
                }

                dispatcher.OpenDevice(device, pcap =>
                {
                    pcap.NonBlocking = true;
                });

                if (hasFilter)
                {
                    dispatcher.Filter = filter!;
                    if (logDeviceDetails)
                    {
                        Log.Information("Npcap[ID:{Index}]: filter set => {Filter}", i, filter);
                    }
                }
                else
                {
                    if (logDeviceDetails)
                    {
                        Log.Information("Npcap[ID:{Index}]: no filter (capturing all)", i);
                    }
                }

                opened++;
            }
            catch (Exception ex)
            {
                if (logDeviceDetails)
                {
                    Log.Error(ex, "Npcap[ID:{Index}]: open failed for {Name}:{Desc}", i, device.Name, device.Description);
                }
                else
                {
                    Log.Debug(ex, "Npcap[ID:{Index}]: recovery open failed for {Name}:{Desc}", i, device.Name, device.Description);
                }
            }
        }

        return opened;
    }


    private void Dispatch(Pcap pcap, ref Packet packet)
    {
        var current = _activePcap;
        if (current is not null && !ReferenceEquals(current, pcap))
        {
            return;
        }

        // L2 (Ethernet)
        var ethReader = new BinaryFormatReader(packet.Data);
        var eth = new L2EthernetFrameShape();
        if (!ethReader.TryReadL2EthernetFrame(ref eth))
        {
            return;
        }

        ushort etherType = (ushort) ((packet.Data[12] << 8) | packet.Data[13]);

        ReadOnlySpan<byte> l3 = eth.Payload;

        if (etherType == 0x0800) // IPv4
        {
            if (TryHandleIPv4Fragment(l3, pcap))
            {
                return;
            }

            var ipReader = new BinaryFormatReader(l3);
            var ip4 = new IPv4PacketShape();
            if (!ipReader.TryReadIPv4Packet(ref ip4))
                return;

            switch ((ProtocolType) ip4.Protocol)
            {
                case ProtocolType.Udp:
                    HandleUdp(ip4.Payload, pcap, GetIPv4SourceAddress(l3));
                    return;

                case ProtocolType.Tcp:
                    return;

                default:
                    return;
            }
        }

        if (etherType == 0x86DD) // IPv6
        {
            if (!TryReadIPv6(l3, out byte nextHeader, out ReadOnlySpan<byte> ip6Payload))
                return;

            switch ((ProtocolType) nextHeader)
            {
                case ProtocolType.Udp:
                    HandleUdp(ip6Payload, pcap, GetIPv6SourceAddress(l3));
                    return;

                case ProtocolType.Tcp:
                    return;

                default:
                    return;
            }
        }
    }

    private bool TryHandleIPv4Fragment(ReadOnlySpan<byte> bytes, Pcap pcap)
    {
        if (!TryReadIPv4Header(bytes, out var header))
        {
            return true;
        }

        if (!header.IsFragmented)
        {
            return false;
        }

        var sourceIp = new IPAddress(bytes.Slice(12, 4).ToArray()).ToString();
        var destinationIp = new IPAddress(bytes.Slice(16, 4).ToArray()).ToString();
        var fragmentPayload = bytes.Slice(header.HeaderLength, header.PayloadLength);
        var reassembledPayload = AddIPv4Fragment(new IPv4FragmentKey(sourceIp, destinationIp, header.Identification, header.Protocol), header, fragmentPayload);

        if (reassembledPayload == null)
        {
            return true;
        }

        HandleReassembledIPv4Payload(header.Protocol, reassembledPayload, pcap, sourceIp);
        return true;
    }

    private byte[]? AddIPv4Fragment(IPv4FragmentKey key, IPv4HeaderInfo header, ReadOnlySpan<byte> fragmentPayload)
    {
        if (header.FragmentOffset < 0
            || fragmentPayload.Length <= 0
            || fragmentPayload.Length > MaxIPv4PayloadLength - header.FragmentOffset)
        {
            return null;
        }

        lock (_ipv4FragmentsLock)
        {
            RemoveExpiredIPv4Fragments();

            if (!_ipv4Fragments.TryGetValue(key, out var fragmentBuffer))
            {
                fragmentBuffer = new IPv4FragmentBuffer();
                _ipv4Fragments[key] = fragmentBuffer;
            }

            fragmentBuffer.LastSeenUtc = DateTime.UtcNow;

            if (!header.HasMoreFragments)
            {
                fragmentBuffer.TotalLength = header.FragmentOffset + fragmentPayload.Length;
            }

            fragmentPayload.CopyTo(fragmentBuffer.Payload.AsSpan(header.FragmentOffset));

            var fragmentEnd = header.FragmentOffset + fragmentPayload.Length;
            for (var index = header.FragmentOffset; index < fragmentEnd; index++)
            {
                if (fragmentBuffer.ReceivedBytes[index])
                {
                    continue;
                }

                fragmentBuffer.ReceivedBytes[index] = true;
                fragmentBuffer.ReceivedBytesCount++;
            }

            if (fragmentBuffer.TotalLength is not { } totalLength
                || fragmentBuffer.ReceivedBytesCount < totalLength)
            {
                TrimIPv4FragmentCache();
                return null;
            }

            var reassembledPayload = fragmentBuffer.Payload.AsSpan(0, totalLength).ToArray();
            _ipv4Fragments.Remove(key);
            return reassembledPayload;
        }
    }

    private void HandleReassembledIPv4Payload(byte protocol, byte[] payload, Pcap pcap, string sourceIp)
    {
        switch ((ProtocolType) protocol)
        {
            case ProtocolType.Udp:
                HandleUdp(payload, pcap, sourceIp);
                return;

            case ProtocolType.Tcp:
                return;

            default:
                return;
        }
    }

    private void RemoveExpiredIPv4Fragments()
    {
        var expirationUtc = DateTime.UtcNow - IPv4FragmentTimeout;
        var expiredKeys = _ipv4Fragments
            .Where(x => x.Value.LastSeenUtc < expirationUtc)
            .Select(x => x.Key)
            .ToList();

        foreach (var expiredKey in expiredKeys)
        {
            _ipv4Fragments.Remove(expiredKey);
        }
    }

    private void TrimIPv4FragmentCache()
    {
        if (_ipv4Fragments.Count <= MaxPendingIPv4FragmentPackages)
        {
            return;
        }

        var keysToRemove = _ipv4Fragments
            .OrderBy(x => x.Value.LastSeenUtc)
            .Take(_ipv4Fragments.Count - MaxPendingIPv4FragmentPackages)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _ipv4Fragments.Remove(key);
        }
    }

    private static bool TryReadIPv4Header(ReadOnlySpan<byte> bytes, out IPv4HeaderInfo header)
    {
        header = default;

        if (bytes.Length < 20)
        {
            return false;
        }

        var version = bytes[0] >> 4;
        if (version != 4)
        {
            return false;
        }

        var headerLength = (bytes[0] & 0x0F) * 4;
        if (headerLength < 20 || bytes.Length < headerLength)
        {
            return false;
        }

        var totalLength = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(2, 2));
        if (totalLength < headerLength || bytes.Length < totalLength)
        {
            return false;
        }

        var identification = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(4, 2));
        var flagsAndFragmentOffset = BinaryPrimitives.ReadUInt16BigEndian(bytes.Slice(6, 2));
        var hasMoreFragments = (flagsAndFragmentOffset & 0x2000) != 0;
        var fragmentOffset = (flagsAndFragmentOffset & 0x1FFF) * 8;

        header = new IPv4HeaderInfo(
            bytes[9],
            identification,
            headerLength,
            totalLength - headerLength,
            hasMoreFragments,
            fragmentOffset);

        return true;
    }

    private static string GetIPv4SourceAddress(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 16)
        {
            return string.Empty;
        }

        return new IPAddress(bytes.Slice(12, 4).ToArray()).ToString();
    }

    private static string GetIPv6SourceAddress(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 24)
        {
            return string.Empty;
        }

        return new IPAddress(bytes.Slice(8, 16).ToArray()).ToString();
    }

    private static bool TryReadIPv6(ReadOnlySpan<byte> bytes, out byte nextHeader, out ReadOnlySpan<byte> payload)
    {
        nextHeader = 0;
        payload = default;

        // IPv6-Header = 40 Bytes
        if (bytes.Length < 40)
        {
            return false;
        }

        // Byte 6 = Next Header
        nextHeader = bytes[6];

        // Payload from Byte 40
        payload = bytes[40..];
        return true;
    }

    private void HandleUdp(ReadOnlySpan<byte> l4Payload, Pcap pcap, string sourceIp)
    {
        var udpReader = new BinaryFormatReader(l4Payload);
        var udp = new UdpPacketShape();
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

        _albionServerDetectionService.DetectFromSourceIp(sourceIp);

        SelectAndMaybeLockAdapter(pcap);

        var current = _activePcap;
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
                }
                else
                {
                    return;
                }
            }

            var score = _pcapScores.GetValueOrDefault(pcap, 0);

            score++;
            _pcapScores[pcap] = score;

            if (score >= ScoreToLock)
            {
                _activePcap = pcap;
                _lastValidPacketUtc = DateTime.UtcNow;
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
            var consecutiveDispatchErrors = 0;

            while (!IsCaptureCancellationRequested())
            {
                PcapDispatcher? dispatcher;
                lock (_dispatcherLock)
                {
                    dispatcher = _dispatcher;
                }

                if (dispatcher is null)
                {
                    TryRecoverDispatcher(null);
                    _cts?.Token.WaitHandle.WaitOne(CaptureRecoveryBackoff);
                    continue;
                }

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

                    if (consecutiveDispatchErrors >= ConsecutiveDispatchErrorsBeforeRecovery)
                    {
                        if (TryRecoverDispatcher(dispatcher))
                        {
                            consecutiveDispatchErrors = 0;
                            continue;
                        }

                        _cts?.Token.WaitHandle.WaitOne(CaptureRecoveryBackoff);
                        continue;
                    }

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

    private bool IsCaptureCancellationRequested()
    {
        var cts = _cts;
        return cts is null || cts.IsCancellationRequested;
    }

    private bool TryRecoverDispatcher(PcapDispatcher? failedDispatcher)
    {
        var cts = _cts;
        if (cts is null || cts.IsCancellationRequested)
        {
            return false;
        }

        ResetAdapterSelection();

        PcapDispatcher? replacement = null;
        var opened = 0;

        try
        {
            replacement = CreateDispatcher();
            opened = OpenConfiguredDevices(replacement, false);

            if (opened == 0)
            {
                replacement.Dispose();
                replacement = null;
            }
        }
        catch (Exception ex)
        {
            replacement?.Dispose();
            LogRecoveryRetryWarning("Npcap: capture recovery failed while reopening devices", ex);
            return false;
        }

        if (replacement is null)
        {
            LogRecoveryRetryWarning("Npcap: capture recovery found no usable devices; retrying", null);
            return false;
        }

        PcapDispatcher? dispatcherToDispose = null;

        lock (_dispatcherLock)
        {
            if (cts.IsCancellationRequested)
            {
                replacement.Dispose();
                return false;
            }

            if (failedDispatcher is null)
            {
                if (_dispatcher is not null)
                {
                    replacement.Dispose();
                    return false;
                }
            }
            else if (!ReferenceEquals(_dispatcher, failedDispatcher))
            {
                replacement.Dispose();
                return false;
            }

            dispatcherToDispose = _dispatcher;
            _dispatcher = replacement;
        }

        dispatcherToDispose?.Dispose();

        _lastRecoveryFailureLogUtc = DateTime.MinValue;
        Log.Information("Npcap: capture recovered on {Opened} device(s)", opened);
        return true;
    }

    private void LogRecoveryRetryWarning(string message, Exception? exception)
    {
        var now = DateTime.UtcNow;

        if (now - _lastRecoveryFailureLogUtc < RecoveryFailureLogInterval)
        {
            return;
        }

        _lastRecoveryFailureLogUtc = now;

        if (exception is null)
        {
            Log.Warning(message);
            return;
        }

        Log.Warning(exception, message);
    }

    private void ResetAdapterSelection()
    {
        lock (_lockObj)
        {
            _activePcap = null;
            _pcapScores.Clear();
            _lastValidPacketUtc = DateTime.MinValue;
        }

        lock (_ipv4FragmentsLock)
        {
            _ipv4Fragments.Clear();
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

            lock (_dispatcherLock)
            {
                _dispatcher?.Dispose();
                _dispatcher = null;
            }

            if (_thread?.IsAlive == true && !_thread.Join(StopThreadJoinTimeout))
            {
                Log.Warning("Npcap: worker did not stop within {TimeoutMs} ms", StopThreadJoinTimeout.TotalMilliseconds);
            }
        }
        finally
        {
            ResetAdapterSelection();
            _cts?.Dispose();
            _cts = null;
            _thread = null;
        }
    }

    public static class PhotonPorts
    {
        public static readonly HashSet<ushort> Udp = [5055, 5056, 5058];
        public static readonly HashSet<ushort> Tcp = [4530, 4531, 4533];
    }

    public static IReadOnlyList<NetworkDeviceInformation> GetAvailableNetworkDevices()
    {
        var result = new List<NetworkDeviceInformation>();
        var devices = Pcap.ListDevices();

        for (int i = 0; i < devices.Count; i++)
        {
            var device = devices[i];

            if (device.Flags.HasFlag(PcapDeviceFlags.Loopback))
            {
                continue;
            }

            if (!device.Flags.HasFlag(PcapDeviceFlags.Up))
            {
                continue;
            }

            var name = string.IsNullOrWhiteSpace(device.Description)
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

    private static string? GetEffectiveFilter()
    {
        var user = SettingsController.CurrentSettings.PacketFilter?.Trim();
        return string.IsNullOrWhiteSpace(user)
            ? null
            : NormalizePacketFilter(user);
    }

    private static string NormalizePacketFilter(string packetFilter)
    {
        return string.Equals(packetFilter, LegacyDefaultPacketFilter, StringComparison.OrdinalIgnoreCase)
            ? DefaultPacketFilter
            : packetFilter;
    }

    private readonly record struct IPv4FragmentKey(string SourceIp, string DestinationIp, ushort Identification, byte Protocol);

    private readonly record struct IPv4HeaderInfo(
        byte Protocol,
        ushort Identification,
        int HeaderLength,
        int PayloadLength,
        bool HasMoreFragments,
        int FragmentOffset)
    {
        public bool IsFragmented => HasMoreFragments || FragmentOffset != 0;
    }

    private sealed class IPv4FragmentBuffer
    {
        public byte[] Payload { get; } = new byte[MaxIPv4PayloadLength];
        public bool[] ReceivedBytes { get; } = new bool[MaxIPv4PayloadLength];
        public int ReceivedBytesCount { get; set; }
        public int? TotalLength { get; set; }
        public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
    }
}