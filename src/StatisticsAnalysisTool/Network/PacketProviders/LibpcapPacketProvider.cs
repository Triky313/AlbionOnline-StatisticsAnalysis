#nullable enable

using BinaryFormat;
using BinaryFormat.EthernetFrame;
using BinaryFormat.IPv4;
using BinaryFormat.Udp;
using Libpcap;
using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using StatisticsAnalysisTool.Abstractions;

namespace StatisticsAnalysisTool.Network.PacketProviders;

public class LibpcapPacketProvider : PacketProvider
{
    private readonly IPhotonReceiver _photonReceiver;
    private readonly PcapDispatcher _dispatcher;
    private CancellationTokenSource? _cts;
    private Thread? _thread;

    public override bool IsRunning => _thread is { IsAlive: true };

    public LibpcapPacketProvider(IPhotonReceiver photonReceiver)
    {
        _photonReceiver = photonReceiver ?? throw new ArgumentNullException(nameof(photonReceiver));

        _dispatcher = new PcapDispatcher(Dispatch);
        _thread = new Thread(Worker)
        {
            IsBackground = true
        };
    }

    public override void Start()
    {
        var devices = Pcap.ListDevices();

        int deviceId = 0;
        foreach (var device in devices)
        {
            if (SettingsController.CurrentSettings.NetworkDevice > 0 && SettingsController.CurrentSettings.NetworkDevice != deviceId)
            {
                Log.Information("NetworkManager (npcap)[ID:{deviceId}]: manually skipping device {Device}:{DeviceDescription}",
                    deviceId++, device.Name, device.Description);
                continue;
            }

            if (device.Type != NetworkInterfaceType.Ethernet && device.Type != NetworkInterfaceType.Wireless80211)
            {
                Log.Information("NetworkManager (npcap)[ID:{deviceId}]: skipping device {Device}:{DeviceDescription} due to unsupported type {Devicetype}",
                    deviceId++, device.Name, device.Description, device.Type);
                continue;
            }
            if (device.Flags.HasFlag(PcapDeviceFlags.Loopback))
            {
                Log.Information("NetworkManager (npcap)[ID:{deviceId}]: skipping device {Device}:{DeviceDescription} due to loopback flag",
                    deviceId++, device.Name, device.Description);
                continue;
            }
            if (!device.Flags.HasFlag(PcapDeviceFlags.Up))
            {
                Log.Information("NetworkManager (npcap)[ID:{deviceId}]: skipping device {Device}:{DeviceDescription} due not being up",
                    deviceId++, device.Name, device.Description);
                continue;
            }

            Log.Information("NetworkManager (npcap)[ID:{deviceId}]: opening device {Device}:{DeviceDescription}",
                deviceId++, device.Name, device.Description);
            _dispatcher.OpenDevice(device, pcap =>
            {
                pcap.NonBlocking = true;
            });

            _dispatcher.Filter = GetEffectiveFilter();
        }

        _cts = new CancellationTokenSource();
        _thread = new Thread(Worker)
        {
            IsBackground = true
        };

        _thread.Start();
    }

    private void Dispatch(Pcap pcap, ref Packet packet)
    {
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

    private static string GetEffectiveFilter()
    {
        var user = SettingsController.CurrentSettings.PacketFilter;
        if (!string.IsNullOrWhiteSpace(user))
        {
            return user;
        }

        return BpfBuilder.BuildDefault(includeTcp: false);
    }
}