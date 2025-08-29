using Serilog;
using StatisticsAnalysisTool.Abstractions;
using StatisticsAnalysisTool.Common;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.PacketProviders;

public class SocketsPacketProvider : PacketProvider
{
    private readonly IPhotonReceiver _photonReceiver;

    private readonly List<Socket> _socketsV4 = [];
    private readonly List<Socket> _socketsV6 = [];

    private readonly List<Task> _receiveTasks = [];
    private readonly ConcurrentBag<byte[]> _buffers = [];
    private volatile bool _stopReceiving;

    public SocketsPacketProvider(IPhotonReceiver photonReceiver)
    {
        _photonReceiver = photonReceiver ?? throw new ArgumentNullException(nameof(photonReceiver));
    }

    public override bool IsRunning => _socketsV4.Any(s => s is { IsBound: true }) || _socketsV6.Any(s => s is { IsBound: true });

    public override void Start()
    {
        _stopReceiving = false;

        var v4 = GetLocalUnicastAddresses(AddressFamily.InterNetwork).ToList();
        var v6 = GetLocalUnicastAddresses(AddressFamily.InterNetworkV6).ToList();

        if (v4.Count == 0 && v6.Count == 0)
        {
            Log.Warning("RawSockets: no local unicast addresses found");
            return;
        }

        // IPv4-Sockets
        foreach (var ip in v4)
        {
            CreateRawSocketIPv4(ip);
        }

        // IPv6-Sockets
        foreach (var ip in v6)
        {
            CreateRawSocketIPv6(ip);
        }

        foreach (var s in _socketsV4.Concat(_socketsV6))
        {
            var buffer = RentBuffer();
            _receiveTasks.Add(Task.Run(() => ReceiveLoopAsync(s, buffer)));
        }
    }

    public override void Stop()
    {
        _stopReceiving = true;

        foreach (var s in _socketsV4.Concat(_socketsV6))
        {
            SafeClose(s);
        }
        _socketsV4.Clear();
        _socketsV6.Clear();

        try
        {
            Task.WaitAll(_receiveTasks.ToArray(), TimeSpan.FromSeconds(2));
        }
        catch
        {
            // ignore
        }

        _receiveTasks.Clear();

        while (_buffers.TryTake(out _)) { }
    }

    private void CreateRawSocketIPv4(IPAddress ip)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
        try
        {
            socket.Bind(new IPEndPoint(ip, 0));

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            var byTrue = new byte[] { 1, 0, 0, 0 };
            var byOut = new byte[4];
            socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);

            _socketsV4.Add(socket);

            DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"RawSocket(v4) added | LocalEndPoint: {socket.LocalEndPoint}, IsBound: {socket.IsBound}, Ttl: {socket.Ttl}");
        }
        catch (SocketException e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Warning(e, "RawSocket(v4) bind/ioctl failed ({Error}) on {IP} - Admin rights required?", e.SocketErrorCode, ip);
            SafeClose(socket);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            SafeClose(socket);
        }
    }

    private void CreateRawSocketIPv6(IPAddress ip)
    {
        var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.IPv6);
        try
        {
            var ep = new IPEndPoint(ip, 0);
            socket.Bind(ep);

            try
            {
                socket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName) 29, true);
            }
            catch
            {
                // ignored
            }

            _socketsV6.Add(socket);

            DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"RawSocket(v6) added | LocalEndPoint: {socket.LocalEndPoint}, IsBound: {socket.IsBound}");
        }
        catch (SocketException e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e); Log.Warning(e, "RawSocket(v6) bind failed ({Error}) on {IP}", e.SocketErrorCode, ip);
            SafeClose(socket);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            SafeClose(socket);
        }
    }

    private async Task ReceiveLoopAsync(Socket socket, byte[] buffer)
    {
        while (!_stopReceiving)
        {
            try
            {
                int bytes = await socket.ReceiveAsync(buffer, SocketFlags.None).ConfigureAwait(false);
                if (bytes > 0)
                {
                    ProcessFrame(buffer.AsSpan(0, bytes), socket.AddressFamily);
                }
            }
            catch (SocketException ex)
            {
                if (_stopReceiving) break;
                DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            }
        }

        ReturnBuffer(buffer);
    }

    private void ProcessFrame(ReadOnlySpan<byte> frame, AddressFamily af)
    {
        if (af == AddressFamily.InterNetwork)
        {
            ProcessIPv4(frame);
        }
        else if (af == AddressFamily.InterNetworkV6)
        {
            ProcessIPv6(frame);
        }
    }

    private void ProcessIPv4(ReadOnlySpan<byte> frame)
    {
        if (frame.Length < 20)
        {
            return;
        }

        byte verIhl = frame[0];
        int version = verIhl >> 4;
        if (version != 4)
        {
            return;
        }

        int ihl = (verIhl & 0x0F) * 4;
        if (ihl < 20 || frame.Length < ihl)
        {
            return;
        }

        ushort flagsFrag = BinaryPrimitives.ReadUInt16BigEndian(frame.Slice(6, 2));
        int fragOffset = (flagsFrag & 0x1FFF) * 8; // under 13 Bits
        if (fragOffset != 0)
        {
            return;
        }

        byte proto = frame[9];
        if (proto != 17)
        {
            return;
        }

        if (frame.Length < ihl + 8)
        {
            return;
        }

        var udp = frame[ihl..];
        ushort srcPort = BinaryPrimitives.ReadUInt16BigEndian(udp);
        ushort dstPort = BinaryPrimitives.ReadUInt16BigEndian(udp[2..]);
        ushort udpLen = BinaryPrimitives.ReadUInt16BigEndian(udp[4..]);

        if (!PhotonPorts.Udp.Contains(srcPort) && !PhotonPorts.Udp.Contains(dstPort))
        {
            int payloadOffset = ihl + 8;
            if (payloadOffset >= frame.Length)
            {
                return;
            }

            var payloadAll = frame[payloadOffset..];
            if (!LooksLikePhoton(payloadAll))
            {
                return;
            }

            int maxPayload = frame.Length - payloadOffset;
            int payloadLen = Math.Min(maxPayload, Math.Max(0, udpLen - 8));
            if (payloadLen <= 0)
            {
                return;
            }

            var payload = frame.Slice(payloadOffset, payloadLen);
            Deliver(payload);
            return;
        }

        // UDP Payload
        int po = ihl + 8;
        int max = frame.Length - po;
        int len = Math.Min(max, Math.Max(0, udpLen - 8));
        if (len <= 0)
        {
            return;
        }

        Deliver(frame.Slice(po, len));
    }

    private void ProcessIPv6(ReadOnlySpan<byte> frame)
    {
        if (frame.Length < 40)
        {
            return;
        }

        int version = (frame[0] >> 4) & 0x0F;
        if (version != 6)
        {
            return;
        }

        ushort payloadLen = BinaryPrimitives.ReadUInt16BigEndian(frame.Slice(4, 2));
        byte nextHeader = frame[6];
        const int l3HeaderLen = 40;

        if (nextHeader != 17)
        {
            return;
        }

        if (frame.Length < l3HeaderLen + 8)
        {
            return;
        }

        var udp = frame[l3HeaderLen..];
        ushort srcPort = BinaryPrimitives.ReadUInt16BigEndian(udp);
        ushort dstPort = BinaryPrimitives.ReadUInt16BigEndian(udp[2..]);
        ushort udpLen = BinaryPrimitives.ReadUInt16BigEndian(udp[4..]);

        int payloadOffset = l3HeaderLen + 8;

        if (udpLen == 0 || udpLen > payloadLen || frame.Length < payloadOffset)
        {
            return;
        }

        int maxPayload = frame.Length - payloadOffset;
        int payloadLenCalc = Math.Min(maxPayload, Math.Max(0, udpLen - 8));
        if (payloadLenCalc <= 0)
        {
            return;
        }

        var payload = frame.Slice(payloadOffset, payloadLenCalc);

        if (!PhotonPorts.Udp.Contains(srcPort) && !PhotonPorts.Udp.Contains(dstPort))
        {
            if (!LooksLikePhoton(payload))
            {
                return;
            }
        }

        Deliver(payload);
    }

    private void Deliver(ReadOnlySpan<byte> payload)
    {
        if (payload.Length == 0)
        {
            return;
        }

        try
        {
            _photonReceiver.ReceivePacket(payload);
        }
        catch (Exception ex)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
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

    private static IEnumerable<IPAddress> GetLocalUnicastAddresses(AddressFamily family)
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Select(ua => ua.Address)
                .Where(a => a.AddressFamily == family)
                .Distinct();
        }
        catch
        {
            return [];
        }
    }

    private static void SafeClose(Socket s)
    {
        try { if (s.Connected) s.Shutdown(SocketShutdown.Both); }
        catch
        {
            // ignored
        }

        try { s.Close(); }
        catch
        {
            // ignored
        }

        try { s.Dispose(); }
        catch
        {
            // ignored
        }
    }

    private byte[] RentBuffer()
    {
        if (_buffers.TryTake(out var buf))
        {
            return buf;
        }

        return new byte[65535];
    }

    private void ReturnBuffer(byte[] buf)
    {
        if (buf.Length == 65535)
        {
            _buffers.Add(buf);
        }
    }

    public static class PhotonPorts
    {
        public static readonly HashSet<ushort> Udp = [5055, 5056, 5058];
    }
}
