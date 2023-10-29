using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Serilog;
using StatisticsAnalysisTool.Common;

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

            socket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, socket);

            _sockets.Add(socket);
            Log.Information("{title}: {message}", "NetworkManager - Added Socket |",
                $"AddressFamily: {socket.AddressFamily}, LocalEndPoint: {socket.LocalEndPoint}, Connected: {socket.Connected}, Available: {socket.Available}, " +
                $"Blocking: {socket.Blocking}, IsBound: {socket.IsBound}, ReceiveBufferSize: {socket.ReceiveBufferSize}, SendBufferSize: {socket.SendBufferSize}, Ttl: {socket.Ttl}");

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

    private void OnReceive(IAsyncResult ar)
    {
        if (_stopReceiving)
        {
            return;
        }

        Socket socket = (Socket) ar.AsyncState;
        socket?.EndReceive(ar);

        using (MemoryStream buffer = new MemoryStream(_byteData))
        {
            using BinaryReader read = new BinaryReader(buffer);
            read.BaseStream.Seek(2, SeekOrigin.Begin);
            ushort dataLength = (ushort) IPAddress.NetworkToHostOrder(read.ReadInt16());

            read.BaseStream.Seek(9, SeekOrigin.Begin);
            int protocol = read.ReadByte();

            if (protocol != 17)
            {
                _byteData = new byte[65000];
                socket?.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, socket);
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
                    byte[] data = read.ReadBytes(dataLength - 28);
                    _ = data.Reverse();

                    try
                    {
                        // TODO: System.OverflowException: 'Arithmetic operation resulted in an overflow.'
                        // TODO: Index was outside the bounds of the array.
                        _photonReceiver.ReceivePacket(data);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        _byteData = new byte[65000];

        if (!_stopReceiving)
        {
            socket?.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, socket);
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