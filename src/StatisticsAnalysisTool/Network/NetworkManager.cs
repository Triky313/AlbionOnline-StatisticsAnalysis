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
using System.Windows;

namespace StatisticsAnalysisTool.Network;

public class NetworkManager
{
    private static IPhotonReceiver _receiver;
    private static readonly List<ICaptureDevice> CapturedDevices = new();
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public static bool IsNetworkCaptureRunning => CapturedDevices.Where(device => device.Started).Any(device => device.Started);
    
    public static bool StartNetworkCapture(TrackingController trackingController)
    {
        ReceiverBuilder builder = ReceiverBuilder.Create();

        builder.AddEventHandler(new NewEquipmentItemEventHandler(trackingController));
        builder.AddEventHandler(new NewSimpleItemEventHandler(trackingController));
        builder.AddEventHandler(new NewFurnitureItemEventHandler(trackingController));
        builder.AddEventHandler(new NewJournalItemEventHandler(trackingController));
        builder.AddEventHandler(new NewLaborerItemEventHandler(trackingController));
        builder.AddEventHandler(new OtherGrabbedLootEventHandler(trackingController));
        builder.AddEventHandler(new InventoryDeleteItemEventHandler(trackingController));
        //builder.AddEventHandler(new InventoryPutItemEventHandler(trackingController));
        builder.AddEventHandler(new TakeSilverEventHandler(trackingController));
        builder.AddEventHandler(new ActionOnBuildingFinishedEventHandler(trackingController));
        builder.AddEventHandler(new UpdateFameEventHandler(trackingController));
        builder.AddEventHandler(new UpdateSilverEventHandler(trackingController));
        builder.AddEventHandler(new UpdateReSpecPointsEventHandler(trackingController));
        builder.AddEventHandler(new UpdateCurrencyEventHandler(trackingController));
        builder.AddEventHandler(new DiedEventHandler(trackingController));
        builder.AddEventHandler(new NewLootChestEventHandler(trackingController));
        builder.AddEventHandler(new UpdateLootChestEventHandler(trackingController));
        builder.AddEventHandler(new LootChestOpenedEventHandler(trackingController));
        builder.AddEventHandler(new InCombatStateUpdateEventHandler(trackingController));
        builder.AddEventHandler(new NewShrineEventHandler(trackingController));
        builder.AddEventHandler(new HealthUpdateEventHandler(trackingController));
        builder.AddEventHandler(new PartyDisbandedEventHandler(trackingController));
        builder.AddEventHandler(new PartyPlayerJoinedEventHandler(trackingController));
        builder.AddEventHandler(new PartyPlayerLeftEventHandler(trackingController));
        builder.AddEventHandler(new PartyChangedOrderEventHandler(trackingController));
        builder.AddEventHandler(new NewCharacterEventHandler(trackingController));
        builder.AddEventHandler(new SiegeCampClaimStartEventHandler(trackingController));
        builder.AddEventHandler(new CharacterEquipmentChangedEventHandler(trackingController));
        builder.AddEventHandler(new NewMobEventHandler(trackingController));
        builder.AddEventHandler(new ActiveSpellEffectsUpdateEventHandler(trackingController));
        builder.AddEventHandler(new UpdateFactionStandingEventHandler(trackingController));
        //builder.AddEventHandler(new ReceivedSeasonPointsEventHandler(trackingController));
        builder.AddEventHandler(new MightFavorPointsEventHandler(trackingController));
        builder.AddEventHandler(new BaseVaultInfoEventHandler(trackingController));
        builder.AddEventHandler(new GuildVaultInfoEventHandler(trackingController));
        builder.AddEventHandler(new NewLootEventHandler(trackingController));
        builder.AddEventHandler(new AttachItemContainerEventHandler(trackingController));
        builder.AddEventHandler(new HarvestFinishedEventHandler(trackingController));

        builder.AddRequestHandler(new InventoryMoveItemRequestHandler(trackingController));
        builder.AddRequestHandler(new UseShrineRequestHandler(trackingController));
        builder.AddRequestHandler(new ReSpecBoostRequestHandler(trackingController));
        builder.AddRequestHandler(new TakeSilverRequestHandler(trackingController));
        builder.AddRequestHandler(new RegisterToObjectRequestHandler(trackingController));
        builder.AddRequestHandler(new UnRegisterFromObjectRequestHandler(trackingController));
        builder.AddRequestHandler(new AuctionBuyOfferRequestHandler(trackingController));
        builder.AddRequestHandler(new AuctionSellSpecificItemRequestHandler(trackingController));

        builder.AddResponseHandler(new ChangeClusterResponseHandler(trackingController));
        builder.AddResponseHandler(new PartyMakeLeaderResponseHandler(trackingController));
        builder.AddResponseHandler(new JoinResponseHandler(trackingController));
        builder.AddResponseHandler(new GetMailInfosResponseHandler(trackingController));
        builder.AddResponseHandler(new ReadMailResponseHandler(trackingController));
        builder.AddResponseHandler(new AuctionGetOffersResponseHandler(trackingController));
        builder.AddResponseHandler(new AuctionGetResponseHandler(trackingController));

        _receiver = builder.Build();
        
        try
        {
            CapturedDevices.AddRange(CaptureDeviceList.Instance);
            return StartDeviceCapture();
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);

            var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
            if (mainWindowViewModel != null)
            {
                mainWindowViewModel.SetErrorBar(Visibility.Visible, LanguageController.Translation("PACKET_HANDLER_ERROR_MESSAGE"));
                _ = mainWindowViewModel.StopTrackingAsync();
            }
            else
            {
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType + " - MainWindowViewModel is null.");
            }

            return false;
        }
    }
    
    private static bool StartDeviceCapture()
    {
        if (CapturedDevices.Count <= 0)
        {
            return false;
        }

        try
        {
            foreach (var device in CapturedDevices)
            {
                PacketEvent(device);
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);

            var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
            if (mainWindowViewModel != null)
            {
                mainWindowViewModel.SetErrorBar(Visibility.Visible, LanguageController.Translation("PACKET_HANDLER_ERROR_MESSAGE"));
                _ = mainWindowViewModel.StopTrackingAsync();
            }
            else
            {
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType + " - MainWindowViewModel is null.");
            }
            
            return false;
        }

        return true;
    }

    public static void StopNetworkCapture()
    {
        foreach (var device in CapturedDevices.Where(device => device.Started))
        {
            device.StopCapture();
            device.Close();
        }

        CapturedDevices.Clear();
    }

    private static void PacketEvent(ICaptureDevice device)
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
        catch (IndexOutOfRangeException ex)
        {
            ConsoleManager.WriteLineForWarning(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
        }
        catch (InvalidOperationException ex)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
        }
        catch (OverflowException ex)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
        }
        catch (ArgumentException ex)
        {
            ConsoleManager.WriteLineForWarning(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(nameof(Device_OnPacketArrival), ex);
        }
    }
}