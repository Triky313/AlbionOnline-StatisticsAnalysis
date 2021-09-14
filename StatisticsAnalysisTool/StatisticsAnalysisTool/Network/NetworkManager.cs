using Albion.Network;
using log4net;
using PacketDotNet;
using SharpPcap;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Handler;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network
{
    public static class NetworkManager
    {
        private static IPhotonReceiver _receiver;
        public static ReceiverBuilder builder;
        private static MainWindowViewModel _mainWindowViewModel;
        private static readonly List<ICaptureDevice> _capturedDevices = new();
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public static bool IsNetworkCaptureRunning => _capturedDevices.Where(device => device.Started).Any(device => device.Started);

        public static bool StartNetworkCapture(MainWindowViewModel mainWindowViewModel, TrackingController trackingController)
        {
            try
            {
                _mainWindowViewModel = mainWindowViewModel;
                builder = ReceiverBuilder.Create();

                //builder.AddResponseHandler(new UseLootChestEventHandler(trackingController));
                builder.AddEventHandler(new NewSimpleItemEventHandler(trackingController));
                builder.AddEventHandler(new NewEquipmentItemEventHandler(trackingController));
                builder.AddEventHandler(new OtherGrabbedLootEventHandler(trackingController));
                builder.AddEventHandler(new InventoryDeleteItemEventHandler(trackingController));
                builder.AddEventHandler(new InventoryPutItemEventHandler(trackingController));
                builder.AddEventHandler(new TakeSilverEventHandler(trackingController));
                builder.AddEventHandler(new UpdateFameEventHandler(trackingController));
                builder.AddEventHandler(new UpdateMoneyEventHandler(trackingController));
                builder.AddEventHandler(new UpdateReSpecPointsEventHandler(trackingController));
                builder.AddEventHandler(new UpdateCurrencyEventHandler(trackingController));
                builder.AddEventHandler(new DiedEventHandler(trackingController));
                builder.AddEventHandler(new NewLootChestEventHandler(trackingController));
                builder.AddEventHandler(new LootChestOpenedEventHandler(trackingController));
                builder.AddEventHandler(new InCombatStateUpdateEventHandler(trackingController));
                builder.AddEventHandler(new NewShrineEventHandler(trackingController));
                builder.AddRequestHandler(new UseShrineEventHandler(trackingController));
                builder.AddResponseHandler(new ChangeClusterResponseHandler(trackingController));
                builder.AddEventHandler(new HealthUpdateEventHandler(trackingController));
                builder.AddEventHandler(new PartyDisbandedEventHandler(trackingController));
                builder.AddEventHandler(new PartyChangedOrderEventHandler(trackingController));
                builder.AddResponseHandler(new PartyMakeLeaderEventHandler(trackingController));
                builder.AddEventHandler(new NewCharacterEventHandler(trackingController));
                builder.AddEventHandler(new SiegeCampClaimStartEventHandler(trackingController));
                builder.AddEventHandler(new NewMobEventHandler(trackingController));
                builder.AddEventHandler(new LeaveEventHandler(trackingController));
                builder.AddEventHandler(new CharacterEquipmentChangedEventHandler(trackingController));
                builder.AddEventHandler(new ActiveSpellEffectsUpdateEventHandler(trackingController));
                builder.AddEventHandler(new PartySilverGainedEventHandler(trackingController));
                builder.AddEventHandler(new UpdateFactionStandingEventHandler(trackingController));
                builder.AddEventHandler(new ReceivedSeasonPointsEventHandler(trackingController));
                
                builder.AddResponseHandler(new JoinResponseHandler(trackingController, _mainWindowViewModel));

                _receiver = builder.Build();

                _capturedDevices.AddRange(CaptureDeviceList.Instance);
                return StartDeviceCapture();
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                _mainWindowViewModel.SetErrorBar(Visibility.Visible, LanguageController.Translation("PACKET_HANDLER_ERROR_MESSAGE"));
                _mainWindowViewModel.StopTracking();
                return false;
            }
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
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                _mainWindowViewModel.SetErrorBar(Visibility.Visible, LanguageController.Translation("PACKET_HANDLER_ERROR_MESSAGE"));
                _mainWindowViewModel.StopTracking();
                return false;
            }

            return true;
        }

        public static void StopNetworkCaptureAsync()
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
                    device.Open(new DeviceConfiguration()
                    {
                        Mode = DeviceModes.Promiscuous,
                        ReadTimeout = 1000
                    });
                    device.OnPacketArrival += Device_OnPacketArrival;
                    device.StartCapture();
                }
            });
        }
        
        private static void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data).Extract<UdpPacket>();
                if (packet != null && (packet.SourcePort == 5056 || packet.DestinationPort == 5056))
                {
                    _receiver.ReceivePacket(packet.PayloadData);
                }
            }
            catch (OverflowException ex)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
                Log.Error(nameof(Device_OnPacketArrival), ex);
            }
            catch (Exception exc)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, exc);
                Log.Error(nameof(Device_OnPacketArrival), exc);
            }
        }
    }
}