using Albion.Network;
using PacketDotNet;
using SharpPcap;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network
{
    public class NetworkController
    {
        private static IPhotonReceiver _receiver;

        public static void StartNetworkCapture()
        {
            var builder = ReceiverBuilder.Create();

            //builder.AddRequestHandler(new MoveRequestHandler());
            //builder.AddEventHandler(new MoveEventHandler());
            //builder.AddEventHandler(new NewCharacterEventHandler());
            builder.AddEventHandler(new TakeSilverEventHandler());
            //builder.AddEventHandler(new NewLootEventHandler());
            //builder.AddEventHandler(new NewLootChestEventHandler());

            _receiver = builder.Build();
            
            var devices = CaptureDeviceList.Instance;
            foreach (var device in devices)
            {
                Task.Run(() =>
                {
                    device.OnPacketArrival += PacketHandler;
                    device.Open(DeviceMode.Promiscuous, 1000);
                    device.StartCapture();
                });
            }
        }

        private static void PacketHandler(object sender, CaptureEventArgs e)
        {
            var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data).Extract<UdpPacket>();
            if (packet != null && (packet.SourcePort == 5056 || packet.DestinationPort == 5056))
            {
                _receiver.ReceivePacket(packet.PayloadData);
            }
        }
    }
}