using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Abstractions;

namespace StatisticsAnalysisTool.Network.PacketProviders;

public class SocketsPacketProvider : PacketProvider
{
    private readonly IPhotonReceiver _photonReceiver;
    private readonly List<Socket> _sockets = new();
    private readonly List<IPAddress> _gateways = new();
    private byte[] _byteData = new byte[65000];
    private bool _stopReceiving;

    public SocketsPacketProvider(IPhotonReceiver photonReceiver)
    {
        _photonReceiver = photonReceiver ?? throw new ArgumentNullException(nameof(photonReceiver));
        var hostEntries = GetAllHostEntries();
        SetGateway(hostEntries);
    }

    public override bool IsRunning => _sockets.Any(IsSocketActive);

    public override void Start()
    {
        _stopReceiving = false;
        foreach (var gateway in _gateways)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            socket.Bind(new IPEndPoint(gateway, 0));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            byte[] byTrue = { 1, 0, 0, 0 };
            byte[] byOut = { 1, 0, 0, 0 };

            try
            {
                socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
            }
            catch (SocketException e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(e, "{message}|{socketErrorCode}", MethodBase.GetCurrentMethod()?.DeclaringType, e.SocketErrorCode);
                continue;
            }

            _ = ReceiveDataAsync(socket); // Start receiving data asynchronously

            _sockets.Add(socket);
            ConsoleManager.WriteLineForMessage($"NetworkManager - Added Socket | AddressFamily: {socket.AddressFamily}, LocalEndPoint: {socket.LocalEndPoint}, " +
                                               $"Connected: {socket.Connected}, Available: {socket.Available}, Blocking: {socket.Blocking}, IsBound: {socket.IsBound}, " +
                                               $"ReceiveBufferSize: {socket.ReceiveBufferSize}, SendBufferSize: {socket.SendBufferSize}, Ttl: {socket.Ttl}");
        }
    }

    public override void Stop()
    {
        _stopReceiving = true;

        foreach (var socket in _sockets)
        {
            if (socket.Connected)
            {
                socket.Disconnect(true);
            }

            socket.Close();
        }

        _sockets.Clear();
    }

    private async Task ReceiveDataAsync(Socket socket)
    {
        while (!_stopReceiving)
        {
            try
            {
                int bytesReceived = await socket.ReceiveAsync(new ArraySegment<byte>(_byteData), SocketFlags.None);
                if (bytesReceived <= 0)
                {
                    ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType, "No data received.", ConsoleColorType.ErrorColor);
                    continue;
                }

                ProcessReceivedData(socket, _byteData, bytesReceived);
            }
            catch (SocketException ex)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            }
            catch (ObjectDisposedException ex)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            }
        }
    }

    private void ProcessReceivedData(Socket socket, byte[] data, int bytesReceived)
    {
        using MemoryStream buffer = new MemoryStream(data, 0, bytesReceived);
        using BinaryReader read = new BinaryReader(buffer);
        read.BaseStream.Seek(2, SeekOrigin.Begin);
        ushort dataLength = (ushort) IPAddress.NetworkToHostOrder(read.ReadInt16());

        read.BaseStream.Seek(9, SeekOrigin.Begin);
        int protocol = read.ReadByte();

        if (protocol != 17)
        {
            return;
        }

        read.BaseStream.Seek(20, SeekOrigin.Begin);

        string srcPort = ((ushort) IPAddress.NetworkToHostOrder(read.ReadInt16())).ToString();
        string destPort = ((ushort) IPAddress.NetworkToHostOrder(read.ReadInt16())).ToString();

        if (srcPort == "5056" || destPort == "5056")
        {
            read.BaseStream.Seek(28, SeekOrigin.Begin);

            if (dataLength >= 28)
            {
                byte[] packetData = read.ReadBytes(dataLength - 28);
                _ = packetData.Reverse();

                try
                {
                    _photonReceiver.ReceivePacket(packetData);
                }
                catch (Exception ex)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
                }
            }
        }
    }

    private static bool IsSocketActive(Socket socket)
    {
        bool part1 = socket.Poll(1000, SelectMode.SelectRead);
        bool part2 = (socket.Available == 0);
        return !part1 || !part2;
    }

    private static IEnumerable<IPHostEntry> GetAllHostEntries()
    {
        List<IPHostEntry> hostEntries = new List<IPHostEntry>();
        string hostName = Dns.GetHostName();
        IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
        hostEntries.Add(hostEntry);
        return hostEntries;
    }

    private void SetGateway(IEnumerable<IPHostEntry> hostEntries)
    {
        foreach (IPAddress ip in hostEntries.SelectMany(hostEntry => hostEntry.AddressList))
        {
            try
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    _gateways.Add(ip);
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}