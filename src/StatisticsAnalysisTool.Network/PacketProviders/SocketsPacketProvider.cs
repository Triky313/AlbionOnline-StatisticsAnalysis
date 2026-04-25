#nullable enable

using Serilog;
using StatisticsAnalysisTool.Abstractions;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.PacketProviders;

[SupportedOSPlatform("windows")]
public sealed class SocketsPacketProvider : PacketProvider
{
    private readonly IPhotonReceiver _photonReceiver;

    private readonly List<Socket> _socketsV4 = [];
    private readonly List<Socket> _socketsV6 = [];
    private readonly List<Task> _receiveTasks = [];
    private readonly ConcurrentBag<byte[]> _buffers = [];

    private volatile bool _stopReceiving;
    private SocketException? _lastSocketException;

    public override bool IsRunning => _socketsV4.Any(x => x is { IsBound: true }) || _socketsV6.Any(x => x is { IsBound: true });

    public SocketsPacketProvider(IPhotonReceiver photonReceiver, NetworkCaptureOptions options)
    {
        _photonReceiver = photonReceiver ?? throw new ArgumentNullException(nameof(photonReceiver));
        ArgumentNullException.ThrowIfNull(options);
    }

    public override void Start()
    {
        _stopReceiving = false;
        _lastSocketException = null;
        UpdateActiveAdapterName(null);

        List<IPAddress> v4 = GetLocalUnicastAddresses(AddressFamily.InterNetwork).ToList();
        List<IPAddress> v6 = GetLocalUnicastAddresses(AddressFamily.InterNetworkV6).ToList();

        if (v4.Count == 0 && v6.Count == 0)
        {
            Log.Warning("RawSockets: no local unicast addresses found");
            throw new NetworkCaptureException("No local network addresses were found for raw socket capture.");
        }

        foreach (IPAddress ip in v4)
        {
            CreateRawSocketIPv4(ip);
        }

        foreach (IPAddress ip in v6)
        {
            CreateRawSocketIPv6(ip);
        }

        if (_socketsV4.Count == 0 && _socketsV6.Count == 0)
        {
            throw _lastSocketException is null
                ? new NetworkCaptureException("Socket capture could not bind to any local network address.")
                : new NetworkCaptureException($"Socket capture could not start (code: {(int)_lastSocketException.SocketErrorCode}). Run the app as Administrator or switch to Npcap.", _lastSocketException);
        }

        List<string> localEndpoints = _socketsV4
            .Concat(_socketsV6)
            .Select(x => x.LocalEndPoint?.ToString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .ToList();

        UpdateActiveAdapterName(localEndpoints.Count switch
        {
            0 => null,
            1 => localEndpoints[0],
            _ => $"{localEndpoints.Count} local sockets"
        });

        foreach (Socket socket in _socketsV4.Concat(_socketsV6))
        {
            byte[] buffer = RentBuffer();
            _receiveTasks.Add(Task.Run(() => ReceiveLoopAsync(socket, buffer)));
        }

        Log.Information("RawSockets: capture started on {Count} local endpoint(s)", localEndpoints.Count);
    }

    public override void Stop()
    {
        _stopReceiving = true;

        foreach (Socket socket in _socketsV4.Concat(_socketsV6))
        {
            SafeClose(socket);
        }

        _socketsV4.Clear();
        _socketsV6.Clear();

        try
        {
            Task.WaitAll(_receiveTasks.ToArray(), TimeSpan.FromSeconds(2));
        }
        catch
        {
        }

        _receiveTasks.Clear();

        while (_buffers.TryTake(out _))
        {
        }

        UpdateActiveAdapterName(null);
    }

    private void CreateRawSocketIPv4(IPAddress ip)
    {
        Socket socket = new(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);

        try
        {
            socket.Bind(new IPEndPoint(ip, 0));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            byte[] byTrue = [1, 0, 0, 0];
            byte[] byOut = new byte[4];
            socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);

            _socketsV4.Add(socket);

            DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"RawSocket(v4) added | LocalEndPoint: {socket.LocalEndPoint}, IsBound: {socket.IsBound}, Ttl: {socket.Ttl}");
        }
        catch (SocketException ex)
        {
            _lastSocketException = ex;
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Warning(ex, "RawSocket(v4) bind/ioctl failed ({Error}) on {IP} - Admin rights required?", ex.SocketErrorCode, ip);
            SafeClose(socket);
        }
        catch (Exception ex)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            SafeClose(socket);
        }
    }

    private void CreateRawSocketIPv6(IPAddress ip)
    {
        Socket socket = new(AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.IPv6);

        try
        {
            IPEndPoint endPoint = new(ip, 0);
            socket.Bind(endPoint);

            try
            {
                socket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)29, true);
            }
            catch
            {
            }

            _socketsV6.Add(socket);

            DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"RawSocket(v6) added | LocalEndPoint: {socket.LocalEndPoint}, IsBound: {socket.IsBound}");
        }
        catch (SocketException ex)
        {
            _lastSocketException = ex;
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Warning(ex, "RawSocket(v6) bind failed ({Error}) on {IP}", ex.SocketErrorCode, ip);
            SafeClose(socket);
        }
        catch (Exception ex)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
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
                if (_stopReceiving)
                {
                    break;
                }

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

    private void ProcessFrame(ReadOnlySpan<byte> frame, AddressFamily addressFamily)
    {
        if (addressFamily == AddressFamily.InterNetwork)
        {
            ProcessIPv4(frame);
            return;
        }

        if (addressFamily == AddressFamily.InterNetworkV6)
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
        bool hasMoreFragments = (flagsFrag & 0x2000) != 0;
        int fragOffset = (flagsFrag & 0x1FFF) * 8;
        if (hasMoreFragments || fragOffset != 0)
        {
            return;
        }

        byte protocol = frame[9];
        if (protocol != 17)
        {
            return;
        }

        if (frame.Length < ihl + 8)
        {
            return;
        }

        ReadOnlySpan<byte> udp = frame[ihl..];
        ushort sourcePort = BinaryPrimitives.ReadUInt16BigEndian(udp);
        ushort destinationPort = BinaryPrimitives.ReadUInt16BigEndian(udp[2..]);
        ushort udpLength = BinaryPrimitives.ReadUInt16BigEndian(udp[4..]);

        if (!PhotonPorts.Udp.Contains(sourcePort) && !PhotonPorts.Udp.Contains(destinationPort))
        {
            int payloadOffset = ihl + 8;
            if (payloadOffset >= frame.Length)
            {
                return;
            }

            ReadOnlySpan<byte> payloadAll = frame[payloadOffset..];
            if (!LooksLikePhoton(payloadAll))
            {
                return;
            }

            int maxPayload = frame.Length - payloadOffset;
            int payloadLength = Math.Min(maxPayload, Math.Max(0, udpLength - 8));
            if (payloadLength <= 0)
            {
                return;
            }

            Deliver(frame.Slice(payloadOffset, payloadLength));
            return;
        }

        int packetOffset = ihl + 8;
        int maxLength = frame.Length - packetOffset;
        int packetLength = Math.Min(maxLength, Math.Max(0, udpLength - 8));
        if (packetLength <= 0)
        {
            return;
        }

        Deliver(frame.Slice(packetOffset, packetLength));
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

        ushort payloadLength = BinaryPrimitives.ReadUInt16BigEndian(frame.Slice(4, 2));
        byte nextHeader = frame[6];
        const int HeaderLength = 40;

        if (nextHeader != 17)
        {
            return;
        }

        if (frame.Length < HeaderLength + 8)
        {
            return;
        }

        ReadOnlySpan<byte> udp = frame[HeaderLength..];
        ushort sourcePort = BinaryPrimitives.ReadUInt16BigEndian(udp);
        ushort destinationPort = BinaryPrimitives.ReadUInt16BigEndian(udp[2..]);
        ushort udpLength = BinaryPrimitives.ReadUInt16BigEndian(udp[4..]);

        int payloadOffset = HeaderLength + 8;

        if (udpLength == 0 || udpLength > payloadLength || frame.Length < payloadOffset)
        {
            return;
        }

        int maxPayload = frame.Length - payloadOffset;
        int payloadLengthCalculated = Math.Min(maxPayload, Math.Max(0, udpLength - 8));
        if (payloadLengthCalculated <= 0)
        {
            return;
        }

        ReadOnlySpan<byte> payload = frame.Slice(payloadOffset, payloadLengthCalculated);

        if (!PhotonPorts.Udp.Contains(sourcePort) && !PhotonPorts.Udp.Contains(destinationPort) && !LooksLikePhoton(payload))
        {
            return;
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

        byte firstByte = payload[0];
        return firstByte is 0xF1 or 0xF2 or 0xFE;
    }

    private static IEnumerable<IPAddress> GetLocalUnicastAddresses(AddressFamily family)
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Select(x => x.Address)
                .Where(x => x.AddressFamily == family)
                .Distinct();
        }
        catch
        {
            return [];
        }
    }

    private static void SafeClose(Socket socket)
    {
        try
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
            }
        }
        catch
        {
        }

        try
        {
            socket.Close();
        }
        catch
        {
        }

        try
        {
            socket.Dispose();
        }
        catch
        {
        }
    }

    private byte[] RentBuffer()
    {
        if (_buffers.TryTake(out byte[]? buffer))
        {
            return buffer;
        }

        return new byte[65535];
    }

    private void ReturnBuffer(byte[] buffer)
    {
        if (buffer.Length == 65535)
        {
            _buffers.Add(buffer);
        }
    }

    public static class PhotonPorts
    {
        public static readonly HashSet<ushort> Udp = [5055, 5056, 5058];
    }
}
