using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Handler;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.PacketProviders;
using StatisticsAnalysisTool.Notification;

namespace StatisticsAnalysisTool.Network;

public class NetworkManager
{
    private readonly PacketProvider _packetProvider;

    public NetworkManager(TrackingController trackingController)
    {
        IPhotonReceiver photonReceiver = Build(trackingController);

        if (SettingsController.CurrentSettings.PacketProvider == PacketProviderKind.Npcap)
        {
            _packetProvider = new LibpcapPacketProvider(photonReceiver);
            Log.Information($"Used packet provider: {PacketProviderKind.Npcap}");
        }
        else
        {
            _packetProvider = new SocketsPacketProvider(photonReceiver);
            Log.Information($"Used packet provider: {PacketProviderKind.Sockets}");
        }
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
        //builder.AddEventHandler(new LootChestOpenedEventHandler(trackingController));
        builder.AddEventHandler(new InCombatStateUpdateEventHandler(trackingController));
        builder.AddEventHandler(new NewShrineEventHandler(trackingController));
        builder.AddEventHandler(new HealthUpdateEventHandler(trackingController));
        builder.AddEventHandler(new HealthUpdatesEventHandler(trackingController));
        builder.AddEventHandler(new PartyDisbandedEventHandler(trackingController));
        builder.AddEventHandler(new PartyJoinedEventHandler(trackingController));
        builder.AddEventHandler(new PartyPlayerJoinedEventHandler(trackingController));
        builder.AddEventHandler(new PartyPlayerLeftEventHandler(trackingController));
        //builder.AddEventHandler(new PartyChangedOrderEventHandler(trackingController));
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
        builder.AddEventHandler(new CraftBuildingInfoEventHandler(trackingController));

        // Request
        builder.AddRequestHandler(new InventoryMoveItemRequestHandler(trackingController));
        builder.AddRequestHandler(new UseShrineRequestHandler(trackingController));
        builder.AddRequestHandler(new ClaimPaymentTransactionRequestHandler(trackingController));
        builder.AddRequestHandler(new ActionOnBuildingStartRequestHandler(trackingController));
        builder.AddRequestHandler(new RegisterToObjectRequestHandler(trackingController));
        builder.AddRequestHandler(new UnRegisterFromObjectRequestHandler(trackingController));
        builder.AddRequestHandler(new AuctionBuyOfferRequestHandler(trackingController));
        builder.AddRequestHandler(new AuctionSellSpecificItemRequestHandler(trackingController));
        builder.AddRequestHandler(new FishingStartEventRequestHandler(trackingController));
        builder.AddRequestHandler(new FishingFinishRequestHandler(trackingController));
        builder.AddRequestHandler(new FishingCancelRequestHandler(trackingController));
        builder.AddRequestHandler(new GetGuildAccountLogsRequestHandler(trackingController));

        // Response
        builder.AddResponseHandler(new ChangeClusterResponseHandler(trackingController));
        builder.AddResponseHandler(new PartyMakeLeaderResponseHandler(trackingController));
        builder.AddResponseHandler(new JoinResponseHandler(trackingController));
        builder.AddResponseHandler(new GetMailInfosResponseHandler(trackingController));
        builder.AddResponseHandler(new ReadMailResponseHandler(trackingController));
        builder.AddResponseHandler(new AuctionGetOffersResponseHandler(trackingController));
        builder.AddResponseHandler(new AuctionGetRequestsResponseHandler(trackingController));
        builder.AddResponseHandler(new GetCharacterEquipmentResponseHandler(trackingController));
        builder.AddResponseHandler(new FishingFinishResponseHandler(trackingController));
        builder.AddResponseHandler(new AuctionGetLoadoutOffersResponseHandler(trackingController));
        builder.AddResponseHandler(new AuctionBuyLoadoutOfferResponseHandler(trackingController));
        builder.AddResponseHandler(new GetGuildAccountLogsResponseHandler(trackingController));

        return builder.Build();
    }

    public void Start()
    {
        ConsoleManager.WriteLineForMessage("Start Capture");

        _packetProvider.Start();

        _ = ServiceLocator.Resolve<SatNotificationManager>().ShowTrackingStatusAsync(LanguageController.Translation("START_TRACKING"), LanguageController.Translation("GAME_TRACKING_IS_STARTED"));
    }

    public void Stop()
    {
        ConsoleManager.WriteLineForMessage("Stop Capture");

        _packetProvider.Stop();

        _ = ServiceLocator.Resolve<SatNotificationManager>().ShowTrackingStatusAsync(LanguageController.Translation("STOP_TRACKING"), LanguageController.Translation("GAME_TRACKING_IS_STOPPED"));
    }

    public bool IsAnySocketActive()
    {
        return _packetProvider.IsRunning;
    }
}