using Serilog;
using StatisticsAnalysisTool.Abstractions;
using StatisticsAnalysisTool.Common;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.PacketProviders;

public class SocketsPacketProvider : PacketProvider
{
    private readonly IPhotonReceiver _photonReceiver;
    private readonly List<Socket> _sockets = [];
    private readonly List<IPAddress> _localAddresses = [];
    private readonly List<Task> _receiveTasks = [];
    private volatile bool _stopReceiving;

    public SocketsPacketProvider(IPhotonReceiver photonReceiver)
    {
        _photonReceiver = photonReceiver ?? throw new ArgumentNullException(nameof(photonReceiver));
        _localAddresses.AddRange(GetLocalIPv4Addresses());
    }

    public override bool IsRunning => _sockets.Any(s => s is { IsBound: true });

    public override void Start()
    {
        _stopReceiving = false;

        foreach (var ip in _localAddresses)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            try
            {
                socket.Bind(new IPEndPoint(ip, 0));
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

                var byTrue = new byte[] { 1, 0, 0, 0 };
                var byOut = new byte[4];
                socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);

                _sockets.Add(socket);

                var buffer = new byte[65535];
                _receiveTasks.Add(Task.Run(() => ReceiveLoopAsync(socket, buffer)));
                ConsoleManager.WriteLineForMessage(
                    $"NetworkManager - Added Raw Socket | LocalEndPoint: {socket.LocalEndPoint}, IsBound: {socket.IsBound}, Ttl: {socket.Ttl}");
            }
            catch (SocketException e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(e, "RawSocket bind/ioctl failed ({Error}) on {IP}", e.SocketErrorCode, ip);
                SafeClose(socket);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                SafeClose(socket);
            }
        }
    }

    public override void Stop()
    {
        _stopReceiving = true;

        foreach (var s in _sockets)
        {
            SafeClose(s);
        }

        _sockets.Clear();

        try
        {
            Task.WaitAll(_receiveTasks.ToArray(), TimeSpan.FromSeconds(2));
        }
        catch { /* ignore */ }

        _receiveTasks.Clear();
    }

    private async Task ReceiveLoopAsync(Socket socket, byte[] buffer)
    {
        while (!_stopReceiving)
        {
            try
            {
                var mem = new ArraySegment<byte>(buffer);
                int bytes = await socket.ReceiveAsync(mem, SocketFlags.None).ConfigureAwait(false);
                if (bytes <= 0)
                {
                    continue;
                }

                ProcessPacket(buffer.AsSpan(0, bytes));
            }
            catch (SocketException ex)
            {
                if (_stopReceiving)
                {
                    break;
                }
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            }
        }
    }

    private void ProcessPacket(ReadOnlySpan<byte> frame)
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

        // Protocol byte
        byte proto = frame[9];
        if (proto != 17) 
        {
            return;
        }

        // UDP header starts after IP header
        if (frame.Length < ihl + 8)
        {
            return;
        }

        var udp = frame.Slice(ihl);
        ushort srcPort = BinaryPrimitives.ReadUInt16BigEndian(udp);
        ushort dstPort = BinaryPrimitives.ReadUInt16BigEndian(udp.Slice(2));
        ushort udpLen = BinaryPrimitives.ReadUInt16BigEndian(udp.Slice(4));
        // ushort udpCrc = BinaryPrimitives.ReadUInt16BigEndian(udp.Slice(6));

        if (!PhotonPorts.Udp.Contains(srcPort) && !PhotonPorts.Udp.Contains(dstPort))
        {
            return;
        }

        // UDP Payload
        int payloadOffset = ihl + 8;
        int maxPayload = frame.Length - payloadOffset;
        int payloadLen = Math.Min(maxPayload, Math.Max(0, udpLen - 8));
        if (payloadLen <= 0)
        {
            return;
        }

        var payload = frame.Slice(payloadOffset, payloadLen);

        try
        {
            _photonReceiver.ReceivePacket(payload);
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
        }
    }

    private static void SafeClose(Socket s)
    {
        try
        {
            if (s.Connected)
            {
                s.Shutdown(SocketShutdown.Both);
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            s.Close();
        }
        catch
        {
            // ignored
        }

        try
        {
            s.Dispose();
        }
        catch
        {
            // ignored
        }
    }

    private static IEnumerable<IPAddress> GetLocalIPv4Addresses()
    {
        try
        {
            var host = Dns.GetHostName();
            var entry = Dns.GetHostEntry(host);
            return entry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).Distinct();
        }
        catch
        {
            return [];
        }
    }

    public static class PhotonPorts
    {
        public static readonly HashSet<ushort> Udp = [5055, 5056, 5058];
    }
}
