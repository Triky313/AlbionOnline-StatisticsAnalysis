using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Handler;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Notification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace StatisticsAnalysisTool.Network;

public class NetworkManager
{
    private readonly IPhotonReceiver _photonReceiver;
    private readonly List<Socket> _sockets = new();
    private byte[] _byteData = new byte[65000];
    private readonly List<IPAddress> _gateways = new();
    private bool _stopReceiving;

    public NetworkManager(TrackingController trackingController)
    {
        _photonReceiver = Build(trackingController);

        var hostEntries = GetAllHostEntries();
        SetGateway(hostEntries);
    }

    private static IPhotonReceiver Build(TrackingController trackingController)
    {
        ReceiverBuilder builder = ReceiverBuilder.Create();

        // Event
        builder.AddEventHandler(new NewEquipmentItemEventHandler(trackingController));
        builder.AddEventHandler(new NewSimpleItemEventHandler(trackingController));
        builder.AddEventHandler(new NewFurnitureItemEventHandler(trackingController));
        builder.AddEventHandler(new NewKillTrophyItemHandler(trackingController));
        builder.AddEventHandler(new NewJournalItemEventHandler(trackingController));
        builder.AddEventHandler(new NewLaborerItemEventHandler(trackingController));
        builder.AddEventHandler(new OtherGrabbedLootEventHandler(trackingController));
        builder.AddEventHandler(new InventoryDeleteItemEventHandler(trackingController));
        //builder.AddEventHandler(new InventoryPutItemEventHandler(trackingController));
        builder.AddEventHandler(new TakeSilverEventHandler(trackingController));
        builder.AddEventHandler(new ActionOnBuildingFinishedEventHandler(trackingController));
        builder.AddEventHandler(new UpdateFameEventHandler(trackingController));
        builder.AddEventHandler(new UpdateMoneyEventHandler(trackingController));
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
        builder.AddEventHandler(new TreasureChestUsingStartEventHandler(trackingController));
        builder.AddEventHandler(new CharacterEquipmentChangedEventHandler(trackingController));
        builder.AddEventHandler(new NewMobEventHandler(trackingController));
        builder.AddEventHandler(new ActiveSpellEffectsUpdateEventHandler(trackingController));
        builder.AddEventHandler(new UpdateFactionStandingEventHandler(trackingController));
        //builder.AddEventHandler(new ReceivedSeasonPointsEventHandler(trackingController));
        builder.AddEventHandler(new MightAndFavorReceivedEventHandler(trackingController));
        builder.AddEventHandler(new BankVaultInfoEventHandler(trackingController));
        builder.AddEventHandler(new GuildVaultInfoEventHandler(trackingController));
        builder.AddEventHandler(new NewLootEventHandler(trackingController));
        builder.AddEventHandler(new AttachItemContainerEventHandler(trackingController));
        builder.AddEventHandler(new HarvestFinishedEventHandler(trackingController));
        builder.AddEventHandler(new RewardGrantedEventHandler(trackingController));
        builder.AddEventHandler(new NewExpeditionCheckPointHandler(trackingController));
        builder.AddEventHandler(new UpdateMistCityStandingEventHandler(trackingController));

        // Request
        builder.AddRequestHandler(new InventoryMoveItemRequestHandler(trackingController));
        builder.AddRequestHandler(new UseShrineRequestHandler(trackingController));
        builder.AddRequestHandler(new ClaimPaymentTransactionRequestHandler(trackingController));
        builder.AddRequestHandler(new TakeSilverRequestHandler(trackingController));
        builder.AddRequestHandler(new RegisterToObjectRequestHandler(trackingController));
        builder.AddRequestHandler(new UnRegisterFromObjectRequestHandler(trackingController));
        builder.AddRequestHandler(new AuctionBuyOfferRequestHandler(trackingController));
        builder.AddRequestHandler(new AuctionSellSpecificItemRequestHandler(trackingController));
        builder.AddRequestHandler(new FishingStartEventRequestHandler(trackingController));
        builder.AddRequestHandler(new FishingFinishRequestHandler(trackingController));
        builder.AddRequestHandler(new FishingCancelRequestHandler(trackingController));

        // Response
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

        return builder.Build();
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

    public void Start()
    {
        ConsoleManager.WriteLineForMessage("Start Capture");

        _stopReceiving = false;
        foreach (var gateway in _gateways)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            socket.Bind(new IPEndPoint(gateway, 0));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            byte[] byTrue = { 1, 0, 0, 0 };
            byte[] byOut = { 1, 0, 0, 0 };

            socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
            socket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);

            _sockets.Add(socket);
        }

        _ = ServiceLocator.Resolve<SatNotificationManager>().ShowTrackingStatusAsync(LanguageController.Translation("START_TRACKING"), LanguageController.Translation("GAME_TRACKING_IS_STARTED"));
    }

    public void Stop()
    {
        ConsoleManager.WriteLineForMessage("Stop Capture");
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
        _ = ServiceLocator.Resolve<SatNotificationManager>().ShowTrackingStatusAsync(LanguageController.Translation("STOP_TRACKING"), LanguageController.Translation("GAME_TRACKING_IS_STOPPED"));
    }

    private void OnReceive(IAsyncResult ar)
    {
        if (_stopReceiving)
        {
            return;
        }

        foreach (var socket in _sockets)
        {
            socket.EndReceive(ar);

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
                    socket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
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
                        // TODO: System.OverflowException: 'Arithmetic operation resulted in an overflow.'
                        _photonReceiver.ReceivePacket(data);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            _byteData = new byte[65000];

            if (!_stopReceiving)
            {
                socket.BeginReceive(_byteData, 0, _byteData.Length, SocketFlags.None, OnReceive, null);
            }
        }
    }

    public bool IsAnySocketActive()
    {
        return _sockets.Any(IsSocketActive);
    }

    private static bool IsSocketActive(Socket socket)
    {
        bool part1 = socket.Poll(1000, SelectMode.SelectRead);
        bool part2 = (socket.Available == 0);
        return !part1 || !part2;
    }
}