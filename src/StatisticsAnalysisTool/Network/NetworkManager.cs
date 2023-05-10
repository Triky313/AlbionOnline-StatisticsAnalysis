using log4net;
using PcapDotNet.Core;
using PcapDotNet.Packets.IpV4;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Handler;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Packet = PcapDotNet.Packets.Packet;

namespace StatisticsAnalysisTool.Network;

public class NetworkManager
{
    private static IPhotonReceiver _receiver;
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private static DateTime _lastGetCurrentServerByIpTime = DateTime.MinValue;
    private static int _serverEventCounter;
    private static AlbionServer _lastServerType;
    private static readonly List<Task> PacketEventTasks = new();
    private static CancellationTokenSource _cancellationTokenSource;

    public static AlbionServer AlbionServer { get; set; } = AlbionServer.Unknown;

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

    public static bool StartDeviceCapture()
    {
        try
        {
            ConsoleManager.WriteLineForMessage("Start Device Capture");

            var capturedDevices = LivePacketDevice.AllLocalMachine;
            if (capturedDevices.Count <= 0)
            {
                ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType, "Error!\nThere are no listening adapters available!");
                return false;
            }

            ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType, "CapturedDevices:");

            _cancellationTokenSource = new CancellationTokenSource();

            foreach (LivePacketDevice selectedDevice in capturedDevices)
            {
                ConsoleManager.WriteLineForMessage($"- {selectedDevice.Description}");

                var task = Task.Run(() => PacketEvent(selectedDevice, _cancellationTokenSource.Token));
                PacketEventTasks.Add(task);
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);

            // TODO: Rework with ServiceLocator.Register
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

    public static void StopDeviceCapture()
    {
        if (_cancellationTokenSource == null)
        {
            return;
        }

        ConsoleManager.WriteLineForMessage("Stop Device Capture");
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;

        PacketEventTasks.Clear();
    }

    private static void PacketEvent(IPacketDevice device, CancellationToken cancellationToken)
    {
        using PacketCommunicator communicator = device.Open(
            65536,
            PacketDeviceOpenAttributes.Promiscuous |
            PacketDeviceOpenAttributes.DataTransferUdpRemote |
            PacketDeviceOpenAttributes.NoCaptureLocal,
            5000);

        using BerkeleyPacketFilter packetFilter = communicator.CreateFilter(SettingsController.CurrentSettings.PacketFilter);
        communicator.SetFilter(packetFilter);

        while (!cancellationToken.IsCancellationRequested)
        {
            communicator.ReceivePackets(1, PacketHandler);
        }
    }

    private static void PacketHandler(Packet packet)
    {
        IpV4Datagram ipV4 = packet.Ethernet.IpV4;

        var server = GetCurrentServerByIpOrSettings(ipV4);
        SetCurrentServer(server);
        if (server == AlbionServer.Unknown)
        {
            return;
        }

        try
        {
            if (ipV4 != null)
            {
                _receiver.ReceivePacket(ipV4.Udp.Payload.ToArray());
            }
        }
        catch (Exception ex) when (ex is IndexOutOfRangeException or InvalidOperationException or ArgumentException)
        {
            ConsoleManager.WriteLineForWarning(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
        }
        catch (OverflowException ex)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            StopDeviceCapture();
        }
        catch (Exception ex)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(nameof(PacketHandler), ex);
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

    private static AlbionServer GetCurrentServerByIpOrSettings(IpV4Datagram ip)
    {
        if (SettingsController.CurrentSettings.Server == 1)
        {
            return AlbionServer.West;
        }

        if (SettingsController.CurrentSettings.Server == 2)
        {
            return AlbionServer.East;
        }

        var srcIp = ip?.CurrentDestination.ToString();
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
        return PacketEventTasks?.Any() ?? false;
    }
}