#nullable enable

using System;
using System.Buffers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using BinaryFormat;
using BinaryFormat.EthernetFrame;
using BinaryFormat.IPv4;
using BinaryFormat.Udp;
using Libpcap;
using Serilog;

namespace StatisticsAnalysisTool.Network.Listeners;

public class LibpcapPacketProvider : PacketProvider
{
    private IPhotonReceiver _photonReceiver;

    private PcapDispatcher _dispatcher;
    private CancellationTokenSource? _cts;
    private Thread _thread;
    
    public override bool IsRunning => _thread.IsAlive;
    
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

        foreach (var device in devices)
        {
            if (device.Type != NetworkInterfaceType.Ethernet && device.Type != NetworkInterfaceType.Wireless80211)
            {
                Log.Information("NetworkManager (npcap): skipping device {Device}:{DeviceDescription} due to unsupported type {Devicetype}", 
                    device.Name, device.Description, device.Type);
                continue;
            }
            if (device.Flags.HasFlag(PcapDeviceFlags.Loopback))
            {
                Log.Information("NetworkManager (npcap): skipping device {Device}:{DeviceDescription} due to loopback flag", 
                    device.Name, device.Description);
                continue;
            }
            if (!device.Flags.HasFlag(PcapDeviceFlags.Up))
            {
                Log.Information("NetworkManager (npcap): skipping device {Device}:{DeviceDescription} due not being up", 
                    device.Name, device.Description);
                continue;
            }

            Log.Information("NetworkManager (npcap): opening device {Device}:{DeviceDescription}", 
                device.Name, device.Description);
            _dispatcher.OpenDevice(device, pcap =>
            {
                pcap.NonBlocking = true;
            });
            _dispatcher.Filter = "udp";
        }
        
        _cts = new CancellationTokenSource();
        _thread.Start();
    }

    private void Dispatch(Pcap pcap, ref Packet packet)
    {
        var ethernetFrameReader = new BinaryFormatReader(packet.Data);
        var ethernetFrame = new L2EthernetFrameShape();
        if (!ethernetFrameReader.TryReadL2EthernetFrame(ref ethernetFrame))
        {
            return;
        }

        var ipv4PacketReader = new BinaryFormatReader(ethernetFrame.Payload);
        var ipv4Packet = new IPv4PacketShape();
        if (!ipv4PacketReader.TryReadIPv4Packet(ref ipv4Packet))
        {
            return;
        }

        if (ipv4Packet.Protocol != (byte)ProtocolType.Udp)
        {
            return;
        }

        var udpPacketReader = new BinaryFormatReader(ipv4Packet.Payload);
        var udpPacket = new UdpPacketShape();
        if (!udpPacketReader.TryReadUdpPacket(ref udpPacket))
        {
            return;
        }

        if (udpPacket.SourcePort != 5056 && udpPacket.DestinationPort != 5056)
        {
            return;
        }

        // TODO: array pool?
        // TODO: better parser?
        try
        {
            // TODO: System.OverflowException: 'Arithmetic operation resulted in an overflow.'
            // TODO: Index was outside the bounds of the array.
            _photonReceiver.ReceivePacket(udpPacket.Payload.ToArray());
        }
        catch
        {
            // ignored
        }
    }

    private void Worker()
    {
        while (_cts is { IsCancellationRequested: false })
        {
            var dispatched = _dispatcher.Dispatch(10);
            if (dispatched <= 0)
            {
                Thread.Sleep(1);
            }
        }
    }

    public override void Stop()
    {
        _dispatcher.Dispose();

        _cts?.Cancel();
        _thread.Join();
        
        _cts?.Dispose();
        _cts = null;
    }
}