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
        private static ReceiverBuilder builder;
        private static MainWindowViewModel _mainWindowViewModel;
        private static readonly List<ICaptureDevice> _capturedDevices = new();
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public static bool IsNetworkCaptureRunning => _capturedDevices.Where(device => device.Started).Any(device => device.Started);

        public static async Task<bool> StartNetworkCaptureAsync(MainWindowViewModel mainWindowViewModel, TrackingController trackingController)
        {
            try
            {
                _mainWindowViewModel = mainWindowViewModel;
                builder = ReceiverBuilder.Create();

                //builder.AddResponseHandler(new UseLootChestEventHandler(trackingController));
                _ = builder.AddEventHandler(new NewSimpleItemEventHandler(trackingController));
                _ = builder.AddEventHandler(new NewEquipmentItemEventHandler(trackingController));
                _ = builder.AddEventHandler(new OtherGrabbedLootEventHandler(trackingController));
                _ = builder.AddEventHandler(new InventoryDeleteItemEventHandler(trackingController));
                _ = builder.AddEventHandler(new InventoryPutItemEventHandler(trackingController));
                _ = builder.AddEventHandler(new TakeSilverEventHandler(trackingController));
                _ = builder.AddEventHandler(new UpdateFameEventHandler(trackingController));
                _ = builder.AddEventHandler(new UpdateSilverEventHandler(trackingController));
                _ = builder.AddEventHandler(new UpdateReSpecPointsEventHandler(trackingController));
                _ = builder.AddEventHandler(new UpdateCurrencyEventHandler(trackingController));
                _ = builder.AddEventHandler(new DiedEventHandler(trackingController));
                _ = builder.AddEventHandler(new NewLootChestEventHandler(trackingController));
                _ = builder.AddEventHandler(new LootChestOpenedEventHandler(trackingController));
                _ = builder.AddEventHandler(new InCombatStateUpdateEventHandler(trackingController));
                _ = builder.AddEventHandler(new NewShrineEventHandler(trackingController));
                _ = builder.AddRequestHandler(new UseShrineEventHandler(trackingController));
                _ = builder.AddResponseHandler(new ChangeClusterResponseHandler(trackingController));
                _ = builder.AddEventHandler(new HealthUpdateEventHandler(trackingController));
                _ = builder.AddEventHandler(new PartyDisbandedEventHandler(trackingController));
                _ = builder.AddEventHandler(new PartyChangedOrderEventHandler(trackingController));
                _ = builder.AddResponseHandler(new PartyMakeLeaderEventHandler(trackingController));
                _ = builder.AddEventHandler(new NewCharacterEventHandler(trackingController));
                _ = builder.AddEventHandler(new SiegeCampClaimStartEventHandler(trackingController));
                _ = builder.AddEventHandler(new NewMobEventHandler(trackingController));
                _ = builder.AddEventHandler(new LeaveEventHandler(trackingController));
                _ = builder.AddEventHandler(new CharacterEquipmentChangedEventHandler(trackingController));
                _ = builder.AddEventHandler(new ActiveSpellEffectsUpdateEventHandler(trackingController));
                _ = builder.AddEventHandler(new PartySilverGainedEventHandler(trackingController));
                _ = builder.AddEventHandler(new UpdateFactionStandingEventHandler(trackingController));
                _ = builder.AddEventHandler(new ReceivedSeasonPointsEventHandler(trackingController));

                _ = builder.AddResponseHandler(new JoinResponseHandler(trackingController, _mainWindowViewModel));

                _receiver = builder.Build();

                _capturedDevices.AddRange(CaptureDeviceList.Instance);
                return await StartDeviceCaptureAsync();
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

        private static async Task<bool> StartDeviceCaptureAsync()
        {
            if (_capturedDevices.Count <= 0)
            {
                return false;
            }

            try
            {
                foreach (var device in _capturedDevices)
                {
                    await PacketEventAsync(device).ConfigureAwait(false);
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

        public static void StopNetworkCapture()
        {
            foreach (var device in _capturedDevices.Where(device => device.Started))
            {
                _ = Task.Run(() =>
                  {
                      device.StopCapture();
                      device.Close();
                      builder = null;
                  });
            }

            _capturedDevices.Clear();
        }

        private static async Task PacketEventAsync(ICaptureDevice device)
        {
            await Task.Run(() =>
            {
                if (!device.Started)
                {
                    device.Open(new DeviceConfiguration()
                    {
                        Mode = DeviceModes.DataTransferUdp,
                        ReadTimeout = 5000
                    });
                    device.OnPacketArrival += Device_OnPacketArrival;
                    device.StartCapture();
                }
            }).ConfigureAwait(false);
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
            catch (InvalidOperationException ioe)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ioe);
                Log.Error(nameof(Device_OnPacketArrival), ioe);
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