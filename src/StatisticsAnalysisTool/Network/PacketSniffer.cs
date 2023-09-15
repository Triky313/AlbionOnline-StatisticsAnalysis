using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network;

public class PacketSniffer
{
    private readonly IPhotonReceiver _photonReceiver;
    private Socket _mainSocket;
    private byte[] _byteData = new byte[65000];
    private readonly IPAddress _gateway;
    
    public PacketSniffer(IPhotonReceiver photonReceiver)
    {
        _photonReceiver = photonReceiver;

        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

        if (hostEntry.AddressList.Length > 0)
        {
            foreach (IPAddress ip in hostEntry.AddressList)
            
            {
                try
                {
                    if (ip.Address != 0)
                    {
                        _gateway = ip;
                        Debug.Print($"{_gateway.AddressFamily} - {_gateway.MapToIPv4()}");
                        break;
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
    }

    public void StartCapture()
    {
        if (_mainSocket != null)
        {
            _mainSocket.Close();
        }

        _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
        _mainSocket.Bind(new IPEndPoint(IPAddress.Parse(_gateway.ToString()), 0));

        _mainSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

        byte[] byTrue = { 1, 0, 0, 0 };
        byte[] byOut = { 1, 0, 0, 0 };

        _mainSocket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
        _mainSocket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
    }

    private void OnReceive(IAsyncResult ar)
    {
        _mainSocket.EndReceive(ar);

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
                _mainSocket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
                return;
            }

            read.BaseStream.Seek(20, SeekOrigin.Begin);

            string srcPort = ((ushort) IPAddress.NetworkToHostOrder(read.ReadInt16())).ToString();
            string destPort = ((ushort) IPAddress.NetworkToHostOrder(read.ReadInt16())).ToString();

            if (srcPort == "5056" || destPort == "5056")
            {
                read.BaseStream.Seek(28, SeekOrigin.Begin);

                byte[] data = read.ReadBytes(dataLength - 28);
                _ = data.Reverse();

                try
                {
                    _photonReceiver.ReceivePacket(data);
                }
                catch
                {
                    // ignored
                }
            }
        }

        _byteData = new byte[65000];

        _mainSocket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
    }
}

//public class SnifferPackets
//{
//    private readonly IPhotonReceiver _photonReceiver;
//    private Socket _mainSocket;
//    private byte[] _byteData = new byte[65000];

//    public SnifferPackets(IPhotonReceiver photonReceiver)
//    {
//        _photonReceiver = photonReceiver;
//    }

//    public void StartCapture(string externalIp = "5.188.125.28")
//    {
//        if (_mainSocket != null)
//        {
//            _mainSocket.Close();
//        }

//        _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
//        _mainSocket.Connect(new IPEndPoint(IPAddress.Parse(externalIp), 0));

//        _mainSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

//        byte[] byTrue = { 1, 0, 0, 0 };
//        byte[] byOut = { 1, 0, 0, 0 };

//        _mainSocket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
//        _mainSocket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
//    }

//    private void OnReceive(IAsyncResult ar)
//    {
//        _mainSocket.EndReceive(ar);

//        using (MemoryStream buffer = new MemoryStream(_byteData))
//        {
//            using BinaryReader read = new BinaryReader(buffer);
//            read.BaseStream.Seek(2, SeekOrigin.Begin);
//            ushort dataLength = (ushort) IPAddress.NetworkToHostOrder(read.ReadInt16());

//            read.BaseStream.Seek(9, SeekOrigin.Begin);
//            int protocol = read.ReadByte();

//            if (protocol != 17)
//            {
//                _byteData = new byte[65000];
//                _mainSocket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
//                return;
//            }

//            read.BaseStream.Seek(20, SeekOrigin.Begin);

//            string srcPort = ((ushort) IPAddress.NetworkToHostOrder(read.ReadInt16())).ToString();
//            string destPort = ((ushort) IPAddress.NetworkToHostOrder(read.ReadInt16())).ToString();

//            if (srcPort == "5056" || destPort == "5056")
//            {
//                read.BaseStream.Seek(28, SeekOrigin.Begin);

//                byte[] data = read.ReadBytes(dataLength - 28);
//                _ = data.Reverse();

//                try
//                {
//                    _photonReceiver.ReceivePacket(data);
//                }
//                catch
//                {
//                    // ignored
//                }
//            }
//        }

//        _byteData = new byte[65000];

//        _mainSocket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
//    }

//    public static IPAddress CreateIpAddress(string baseIp, int lastOctet)
//    {
//        if (lastOctet is < 0 or > 255)
//        {
//            throw new ArgumentOutOfRangeException(nameof(lastOctet), "Last octet must be between 0 and 255.");
//        }

//        string fullIp = $"{baseIp}.{lastOctet}";
//        return IPAddress.Parse(fullIp);
//    }
//}