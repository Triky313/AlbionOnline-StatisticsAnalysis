using PacketDotNet;
using Serilog;
using SharpPcap;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Network.Handler;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Notification;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network;

public static class NetworkManager
{
    private static IPhotonReceiver _receiver;
    private static DateTime _lastGetCurrentServerByIpTime = DateTime.MinValue;
    private static int _serverEventCounter;
    private static AlbionServer _lastServerType;
    private static readonly List<ICaptureDevice> CapturedDevices = new();

    public static AlbionServer AlbionServer { get; set; } = AlbionServer.Unknown;

    public static void StartNetworkCapture(TrackingController trackingController)
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
        //builder.AddEventHandler(new PartyJoinedEventHandler(trackingController));
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
        builder.AddEventHandler(new MightAndFavorReceivedEventHandler(trackingController));
        builder.AddEventHandler(new BaseVaultInfoEventHandler(trackingController));
        builder.AddEventHandler(new GuildVaultInfoEventHandler(trackingController));
        builder.AddEventHandler(new NewLootEventHandler(trackingController));
        builder.AddEventHandler(new AttachItemContainerEventHandler(trackingController));
        builder.AddEventHandler(new HarvestFinishedEventHandler(trackingController));
        builder.AddEventHandler(new RewardGrantedEventHandler(trackingController));
        builder.AddEventHandler(new NewExpeditionCheckPointHandler(trackingController));
        builder.AddEventHandler(new UpdateBrecilienStandingEventHandler(trackingController));

        builder.AddRequestHandler(new InventoryMoveItemRequestHandler(trackingController));
        builder.AddRequestHandler(new UseShrineRequestHandler(trackingController));
        builder.AddRequestHandler(new ReSpecBoostRequestHandler(trackingController));
        builder.AddRequestHandler(new TakeSilverRequestHandler(trackingController));
        builder.AddRequestHandler(new RegisterToObjectRequestHandler(trackingController));
        builder.AddRequestHandler(new UnRegisterFromObjectRequestHandler(trackingController));
        builder.AddRequestHandler(new AuctionBuyOfferRequestHandler(trackingController));
        builder.AddRequestHandler(new AuctionSellSpecificItemRequestHandler(trackingController));
        builder.AddRequestHandler(new FishingStartEventRequestHandler(trackingController));
        builder.AddRequestHandler(new FishingFinishRequestHandler(trackingController));
        builder.AddRequestHandler(new FishingCancelRequestHandler(trackingController));

        builder.AddResponseHandler(new ChangeClusterResponseHandler(trackingController));
        builder.AddResponseHandler(new PartyMakeLeaderResponseHandler(trackingController));
        builder.AddResponseHandler(new JoinResponseHandler(trackingController));
        builder.AddResponseHandler(new GetMailInfosResponseHandler(trackingController));
        builder.AddResponseHandler(new ReadMailResponseHandler(trackingController));
        builder.AddResponseHandler(new AuctionGetOffersResponseHandler(trackingController));
        builder.AddResponseHandler(new AuctionGetResponseHandler(trackingController));
        builder.AddResponseHandler(new GetCharacterEquipmentResponseHandler(trackingController));
        builder.AddResponseHandler(new FishingFinishResponseHandler(trackingController));
        builder.AddResponseHandler(new AuctionGetLoadoutOffersResponseHandler(trackingController));
        builder.AddResponseHandler(new AuctionBuyLoadoutOfferResponseHandler(trackingController));

        _receiver = builder.Build();
        StartDeviceCapture();
    }

    public static void StartDeviceCapture()
    {
        ConsoleManager.WriteLineForMessage("Start Device Capture");

        CapturedDevices.Clear();

        foreach (var device in CaptureDeviceList.Instance)
        {
            if (device.Started)
            {
                continue;
            }

            try
            {
                device.Open(new DeviceConfiguration()
                {
                    Mode = DeviceModes.DataTransferUdp | DeviceModes.Promiscuous | DeviceModes.NoCaptureLocal,
                    ReadTimeout = 5000
                });
            }
            catch (Exception e)
            {
                Log.Warning(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
                continue;
            }

            CapturedDevices.Add(device);
        }

        if (CapturedDevices.Count <= 0)
        {
            throw new NoListeningAdaptersException();
        }

        ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType, "CapturedDevices:");

        foreach (ICaptureDevice captureDevice in CapturedDevices.ToList())
        {
            ConsoleManager.WriteLineForMessage($"- {captureDevice.Description}");
            PacketEvent(captureDevice);
        }

        _ = ServiceLocator.Resolve<SatNotificationManager>().ShowTrackingStatusAsync(LanguageController.Translation("START_TRACKING"), LanguageController.Translation("GAME_TRACKING_IS_STARTED"));
    }

    public static void StopDeviceCapture()
    {
        if (CapturedDevices is not { Count: > 0 })
        {
            return;
        }

        ConsoleManager.WriteLineForMessage("Stop Device Capture");

        foreach (ICaptureDevice capturedDevice in CapturedDevices)
        {
            capturedDevice.StopCapture();
            capturedDevice.OnPacketArrival -= Device_OnPacketArrival;
            capturedDevice.Close();
        }

        CapturedDevices.Clear();

        _ = ServiceLocator.Resolve<SatNotificationManager>().ShowTrackingStatusAsync(LanguageController.Translation("STOP_TRACKING"), LanguageController.Translation("GAME_TRACKING_IS_STOPPED"));
    }

    private static void PacketEvent(ICaptureDevice device)
    {
        if (device.Started)
        {
            return;
        }

        device.Filter = SettingsController.CurrentSettings.PacketFilter;
        device.OnPacketArrival += Device_OnPacketArrival;
        device.StartCapture();
    }

    private static void Device_OnPacketArrival(object sender, PacketCapture e)
    {
        try
        {
            var server = GetCurrentServerByIpOrSettings(e);
            SetCurrentServer(server);
            if (server == AlbionServer.Unknown)
            {
                return;
            }

            var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data).Extract<UdpPacket>();
            if (packet != null)
            {
                _receiver.ReceivePacket(packet.PayloadData);
            }
        }
        catch (Exception ex) when (ex is IndexOutOfRangeException or InvalidOperationException or ArgumentException)
        {
            ConsoleManager.WriteLineForWarning(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
        }
        catch (OverflowException ex)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    public static void SetCurrentServer(AlbionServer server, bool directUpdateWithoutCounter = false)
    {
        if (directUpdateWithoutCounter)
        {
            AlbionServer = server;
            _ = UpdateMainWindowServerTypeLabelAsync(server);
            return;
        }

        if ((DateTime.Now - _lastGetCurrentServerByIpTime).TotalSeconds < 10)
        {
            return;
        }

        AlbionServer = server;
        _ = UpdateMainWindowServerTypeLabelAsync(server);

        _lastGetCurrentServerByIpTime = DateTime.Now;
    }

    private static AlbionServer GetCurrentServerByIpOrSettings(PacketCapture e)
    {
        if (SettingsController.CurrentSettings.Server == 1)
        {
            return AlbionServer.West;
        }

        if (SettingsController.CurrentSettings.Server == 2)
        {
            return AlbionServer.East;
        }

        var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
        var ipPacket = packet.Extract<IPPacket>();
        var srcIp = ipPacket?.SourceAddress?.ToString();
        var albionServer = AlbionServer.Unknown;

        if (srcIp == null || string.IsNullOrEmpty(srcIp))
        {
            albionServer = AlbionServer.Unknown;
        }
        else if (srcIp.Contains("5.188.125."))
        {
            albionServer = AlbionServer.West;
        }
        else if (srcIp!.Contains("5.45.187."))
        {
            albionServer = AlbionServer.East;
        }

        return GetActiveAlbionServer(albionServer);
    }

    private static AlbionServer GetActiveAlbionServer(AlbionServer albionServer)
    {
        if (albionServer != AlbionServer.Unknown && _lastServerType == albionServer)
        {
            _serverEventCounter++;
        }
        else if (albionServer != AlbionServer.Unknown)
        {
            _serverEventCounter = 1;
            _lastServerType = albionServer;
        }

        if (_serverEventCounter < 20 || albionServer == AlbionServer.Unknown)
        {
            return _lastServerType;
        }

        _serverEventCounter = 20;
        return albionServer;
    }

    private static async Task UpdateMainWindowServerTypeLabelAsync(AlbionServer albionServer)
    {
        await Task.Run(() =>
        {
            var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
            mainWindowViewModel.ServerTypeText = albionServer switch
            {
                AlbionServer.East => LanguageController.Translation("EAST_SERVER"),
                AlbionServer.West => LanguageController.Translation("WEST_SERVER"),
                _ => LanguageController.Translation("UNKNOWN_SERVER")
            };
        });
    }

    public static void RestartNetworkCapture()
    {
        StopDeviceCapture();
        StartDeviceCapture();
    }

    public static bool IsNetworkCaptureRunning()
    {
        return CapturedDevices?.Any(x => x.Started) ?? false;
    }
}