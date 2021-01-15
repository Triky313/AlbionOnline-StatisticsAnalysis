using Albion.Network;
using PacketDotNet;
using SharpPcap;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Handler;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network
{
    public static class NetworkController
    {
        private static IPhotonReceiver _receiver;
        private static readonly List<ICaptureDevice> _capturedDevices = new List<ICaptureDevice>();
        public static ReceiverBuilder builder;
        private static MainWindowViewModel _mainWindowViewModel;

        public static void StartNetworkCapture(MainWindowViewModel mainWindowViewModel, TrackingController trackingController, FameCountUpTimer fameCountUpTimer)
        {
            _mainWindowViewModel = mainWindowViewModel;
            builder = ReceiverBuilder.Create();


            //builder.AddRequestHandler(new UserInformationHandler());
            //builder.AddEventHandler(new NewRandomDungeonExitEventHandler());
            //builder.AddEventHandler(new NewCharacterEventHandler());

            //builder.AddEventHandler(new TakeSilverEventHandler()); // GEHT
            builder.AddEventHandler(new UpdateFameEventHandler(_mainWindowViewModel, trackingController, fameCountUpTimer)); // GEHT

            builder.AddEventHandler(new NewRandomDungeonExitEventHandler());

            //builder.AddEventHandler(new PartySilverGainedEventHandler());
            
            //builder.AddEventHandler(new NewLootEventHandler());
            //builder.AddEventHandler(new NewLootChestEventHandler());

            builder.AddResponseHandler(new UserInformationHandler(_mainWindowViewModel)); // GEHT

            _receiver = builder.Build();

            _capturedDevices.AddRange(CaptureDeviceList.Instance);
            StartDeviceCapture();
        }

        private static void StartDeviceCapture()
        {
            foreach (var device in _capturedDevices)
            {
                Task.Run(() =>
                {
                    if (!device.Started)
                    {
                        device.OnPacketArrival += PacketHandler;
                        device.Open(DeviceMode.Promiscuous, 1000);
                        device.StartCapture();
                    }
                });
            }
        }

        public static void StopNetworkCapture()
        {
            foreach (var device in _capturedDevices.Where(device => device.Started))
            {
                Task.Run(() =>
                {
                    device.StopCapture();
                    device.Close();
                    builder = null;
                });
            }
            _capturedDevices.Clear();
        }

        public static bool IsNetworkCaptureRunning => _capturedDevices.Where(device => device.Started).Any(device => device.Started);

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