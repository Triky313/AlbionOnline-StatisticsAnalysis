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
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace StatisticsAnalysisTool.Network.PacketProviders;

public class LibpcapPacketProvider : PacketProvider
{
    private readonly IPhotonReceiver _photonReceiver;
    private readonly PcapDispatcher _dispatcher;
    private CancellationTokenSource? _cts;
    private Thread? _thread;
    private volatile Pcap? _activePcap;
    private readonly bool _lockToFirstDevice = true;

    public override bool IsRunning => _thread is { IsAlive: true };

    public LibpcapPacketProvider(IPhotonReceiver photonReceiver)
    {
        _photonReceiver = photonReceiver ?? throw new ArgumentNullException(nameof(photonReceiver));
        _dispatcher = new PcapDispatcher(Dispatch);
    }

    public override void Start()
    {
        if (_thread is { IsAlive: true })
        {
            return;
        }

        _activePcap = null;

        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        var devices = Pcap.ListDevices();
        if (devices.Count == 0)
        {
            Log.Warning("Npcap: no devices found");
            return;
        }

        var filter = GetEffectiveFilter();
        bool hasFilter = !string.IsNullOrWhiteSpace(filter);

        int configuredIndex = SettingsController.CurrentSettings.NetworkDevice;
        int opened = 0;
        for (int i = 0; i < devices.Count; i++)
        {
            var device = devices[i];

            if (configuredIndex >= 0 && i != configuredIndex)
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

            try
            {
                Log.Information("Npcap[ID:{Index}]: opening {Name}:{Desc} (Type={Type}, Flags={Flags})",
                    i, device.Name, device.Description, device.Type, device.Flags);

                _dispatcher.OpenDevice(device, pcap =>
                {
                    pcap.NonBlocking = true;
                });

                if (hasFilter)
                {
                    _dispatcher.Filter = filter!;
                    Log.Information("Npcap[ID:{Index}]: filter set => {Filter}", i, filter);
                }
                else
                {
                    Log.Information("Npcap[ID:{Index}]: no filter (capturing all)", i);
                }

                opened++;

                if (configuredIndex >= 0)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Npcap[ID:{Index}]: open failed for {Name}:{Desc}", i, device.Name, device.Description);
            }
        }

        if (opened == 0)
        {
            Log.Warning("Npcap: no device opened (check NetworkDevice index or admin rights)");
            return;
        }

        _thread = new Thread(Worker) { IsBackground = true };
        _thread.Start();

        Log.Information("Npcap: capture started on {Opened} device(s), filter: {Filter}", opened, hasFilter ? filter : "<none>");
    }


    private void Dispatch(Pcap pcap, ref Packet packet)
    {
        if (_lockToFirstDevice)
        {
            var current = _activePcap;
            if (current is null)
            {
                _activePcap = pcap;
            }
            else if (!ReferenceEquals(current, pcap))
            {
                return;
            }
        }

        var ethReader = new BinaryFormatReader(packet.Data);
        var eth = new L2EthernetFrameShape();
        if (!ethReader.TryReadL2EthernetFrame(ref eth))
        {
            return;
        }

        var ipReader = new BinaryFormatReader(eth.Payload);
        var ip = new IPv4PacketShape();
        if (!ipReader.TryReadIPv4Packet(ref ip))
        {
            return;
        }

        switch ((ProtocolType) ip.Protocol)
        {
            case ProtocolType.Udp:
            {
                var udpReader = new BinaryFormatReader(ip.Payload);
                var udp = new UdpPacketShape();
                if (!udpReader.TryReadUdpPacket(ref udp))
                {
                    return;
                }

                if (!PhotonPorts.Udp.Contains(udp.SourcePort) && !PhotonPorts.Udp.Contains(udp.DestinationPort))
                {
                    return;
                }

                if (_lockToFirstDevice)
                {
                    var current = _activePcap;
                    if (current is null)
                    {
                        _activePcap = pcap;
                    }
                    else if (!ReferenceEquals(current, pcap))
                    {
                        return;
                    }
                }

                if (udp.Payload.Length == 0)
                {
                    return;
                }

                try
                {
                    _photonReceiver.ReceivePacket(udp.Payload);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "PhotonReceiver.ReceivePacket failed");
                }
                return;
            }

            case ProtocolType.Tcp:
                {
                    return;
                }

            default:
                {
                    return;
                }
        }
    }

    private void Worker()
    {
        try
        {
            while (_cts is { IsCancellationRequested: false })
            {
                int dispatched;
                try
                {
                    dispatched = _dispatcher.Dispatch(50);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (InvalidOperationException)
                {
                    break;
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

    public override void Stop()
    {
        try
        {
            _cts?.Cancel();
            _dispatcher.Dispose();
            _thread?.Join();
        }
        finally
        {
            _activePcap = null;
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

    public static class BpfBuilder
    {
        public static string BuildDefault(bool includeTcp)
        {
            var udp = "(udp and (port 5055 or port 5056 or port 5058))";
            if (!includeTcp)
            {
                return udp;
            }

            var tcp = "(tcp and (port 4530 or port 4531 or port 4533))";
            return $"({udp}) or ({tcp})";
        }
    }

    private static string? GetEffectiveFilter()
    {
        var user = SettingsController.CurrentSettings.PacketFilter;
        return string.IsNullOrWhiteSpace(user) ? null : user;
    }
}