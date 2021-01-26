using Albion.Network;
using log4net;
using PacketDotNet;
using SharpPcap;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Handler;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network
{
    public static class NetworkController
    {
        private static IPhotonReceiver _receiver;
        public static ReceiverBuilder builder;
        private static MainWindowViewModel _mainWindowViewModel;
        private static readonly List<ICaptureDevice> _capturedDevices = new List<ICaptureDevice>();
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool StartNetworkCapture(MainWindowViewModel mainWindowViewModel, TrackingController trackingController, ValueCountUpTimer valueCountUpTimerTimer)
        {
            _mainWindowViewModel = mainWindowViewModel;
            builder = ReceiverBuilder.Create();

            //builder.AddRequestHandler(new UserInformationHandler());
            //builder.AddEventHandler(new UpdateMoneyEventHandler());
            //builder.AddEventHandler(new NewCharacterEventHandler());

            //builder.AddEventHandler(new TakeSilverEventHandler()); // GEHT
            builder.AddEventHandler(new UpdateFameEventHandler(trackingController, valueCountUpTimerTimer.FameCountUpTimer));
            builder.AddEventHandler(new UpdateMoneyEventHandler(trackingController, valueCountUpTimerTimer.SilverCountUpTimer));
            builder.AddEventHandler(new UpdateReSpecPointsEventHandler(trackingController, valueCountUpTimerTimer.ReSpecPointsCountUpTimer));

            //builder.AddEventHandler(new PartySilverGainedEventHandler());

            //builder.AddEventHandler(new NewLootEventHandler());

            builder.AddResponseHandler(new UserInformationHandler(_mainWindowViewModel)); // GEHT

            _receiver = builder.Build();

            _capturedDevices.AddRange(CaptureDeviceList.Instance);
            return StartDeviceCapture();
        }

        private static bool StartDeviceCapture()
        {
            if (_capturedDevices.Count <= 0)
            {
                return false;
            }

            try
            {
                foreach (var device in _capturedDevices)
                {
                    PacketEvent(device);
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(StartDeviceCapture), e);
                _mainWindowViewModel.SetErrorBar(Visibility.Visible, LanguageController.Translation("PACKET_HANDLER_ERROR_MESSAGE"));
                _mainWindowViewModel.StopTracking();
                return false;
            }
            
            return true;
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

        private static async void PacketEvent(ICaptureDevice device)
        {
            await Task.Run(() =>
            {
                if (!device.Started)
                {
                    device.OnPacketArrival += PacketHandler;
                    device.Open(DeviceMode.Promiscuous, 1000);
                    device.StartCapture();
                }
            });
        }

        public static bool IsNetworkCaptureRunning => _capturedDevices.Where(device => device.Started).Any(device => device.Started);

        private static void PacketHandler(object sender, CaptureEventArgs e)
        {
            var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data).Extract<UdpPacket>();
            if (packet != null && (packet.SourcePort == 5056 || packet.DestinationPort == 5056))
            {
                // TODO: System.ArgumentException: "DependencySource" 
                // https://stackoverflow.com/questions/26361020/error-must-create-dependencysource-on-same-thread-as-the-dependencyobject-even
                _receiver.ReceivePacket(packet.PayloadData);
            }
        }
    }
}