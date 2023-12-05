using Serilog;
using StatisticsAnalysisTool.Avalonia.Network.PacketProviders;
using StatisticsAnalysisTool.Avalonia.ToolSettings;
using StatisticsAnalysisTool.Network;

namespace StatisticsAnalysisTool.Avalonia.Network;

public class NetworkManager : INetworkManager
{
    private readonly PacketProvider _packetProvider;

    public NetworkManager(ISettingsController settingsController)
    {
        IPhotonReceiver photonReceiver = Build();

        if (settingsController.CurrentUserSettings.PacketProvider == PacketProviderKind.Npcap)
        {
            _packetProvider = new LibpcapPacketProvider(photonReceiver, settingsController);
            Log.Information($"Used packet provider: {PacketProviderKind.Npcap}");
        }
        else
        {
            _packetProvider = new SocketsPacketProvider(photonReceiver);
            Log.Information($"Used packet provider: {PacketProviderKind.Sockets}");
        }
    }

    private IPhotonReceiver Build()
    {
        ReceiverBuilder builder = ReceiverBuilder.Create();

        //// Event
        //builder.AddEventHandler(new NewEquipmentItemEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new NewSimpleItemEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new NewFurnitureItemEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new NewKillTrophyItemHandler(_gameEventWrapper));
        //builder.AddEventHandler(new NewJournalItemEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new NewLaborerItemEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new OtherGrabbedLootEventHandler(_gameEventWrapper.LootController));
        //builder.AddEventHandler(new TakeSilverEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new ActionOnBuildingFinishedEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new UpdateFameEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new UpdateReSpecPointsEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new UpdateCurrencyEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new DiedEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new NewLootChestEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new UpdateLootChestEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new InCombatStateUpdateEventHandler(_gameEventWrapper.CombatController));
        //builder.AddEventHandler(new NewShrineEventHandler(_gameEventWrapper.DungeonController));
        //builder.AddEventHandler(new HealthUpdateEventHandler(_gameEventWrapper.CombatController));
        //builder.AddEventHandler(new PartyDisbandedEventHandler(_gameEventWrapper.EntityController));
        //builder.AddEventHandler(new PartyJoinedEventHandler(_gameEventWrapper.EntityController));
        //builder.AddEventHandler(new PartyPlayerJoinedEventHandler(_gameEventWrapper.EntityController));
        //builder.AddEventHandler(new PartyPlayerLeftEventHandler(_gameEventWrapper.EntityController));
        //builder.AddEventHandler(new NewCharacterEventHandler(_gameEventWrapper.EntityController));
        //builder.AddEventHandler(new CharacterEquipmentChangedEventHandler(_gameEventWrapper.EntityController));
        //builder.AddEventHandler(new NewMobEventHandler(_gameEventWrapper.DungeonController));
        //builder.AddEventHandler(new ActiveSpellEffectsUpdateEventHandler(_gameEventWrapper.EntityController));
        //builder.AddEventHandler(new UpdateFactionStandingEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new MightAndFavorReceivedEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new BankVaultInfoEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new NewLootEventHandler(_gameEventWrapper.LootController));
        //builder.AddEventHandler(new AttachItemContainerEventHandler(_gameEventWrapper));
        //builder.AddEventHandler(new HarvestFinishedEventHandler(_gameEventWrapper.GatheringController));
        //builder.AddEventHandler(new RewardGrantedEventHandler(_gameEventWrapper.GatheringController));
        //builder.AddEventHandler(new NewExpeditionCheckPointHandler(_gameEventWrapper.DungeonController));
        //builder.AddEventHandler(new UpdateMistCityStandingEventHandler(_gameEventWrapper.DungeonController));
        //builder.AddEventHandler(new CraftBuildingInfoEventHandler(_gameEventWrapper.TradeController));

        //// Request
        //builder.AddRequestHandler(new InventoryMoveItemRequestHandler(_gameEventWrapper));
        //builder.AddRequestHandler(new ClaimPaymentTransactionRequestHandler(_gameEventWrapper.EntityController));
        //builder.AddRequestHandler(new ActionOnBuildingStartRequestHandler(_gameEventWrapper));
        //builder.AddRequestHandler(new RegisterToObjectRequestHandler(_gameEventWrapper));
        //builder.AddRequestHandler(new UnRegisterFromObjectRequestHandler(_gameEventWrapper));
        //builder.AddRequestHandler(new AuctionBuyOfferRequestHandler(_gameEventWrapper.MarketController));
        //builder.AddRequestHandler(new AuctionSellSpecificItemRequestHandler(_gameEventWrapper.MarketController));
        //builder.AddRequestHandler(new FishingStartEventRequestHandler(_gameEventWrapper.GatheringController));
        //builder.AddRequestHandler(new FishingFinishRequestHandler(_gameEventWrapper.GatheringController));
        //builder.AddRequestHandler(new FishingCancelRequestHandler(_gameEventWrapper.GatheringController));
        //builder.AddRequestHandler(new GetGuildAccountLogsRequestHandler(_gameEventWrapper.GuildController));

        //// Response
        //builder.AddResponseHandler(new ChangeClusterResponseHandler(_gameEventWrapper));
        //builder.AddResponseHandler(new JoinResponseHandler(_gameEventWrapper, _mainWindowViewModel));
        //builder.AddResponseHandler(new GetMailInfosResponseHandler(_gameEventWrapper.MailController));
        //builder.AddResponseHandler(new ReadMailResponseHandler(_gameEventWrapper.MailController));
        //builder.AddResponseHandler(new AuctionGetOffersResponseHandler(_gameEventWrapper.MarketController));
        //builder.AddResponseHandler(new AuctionGetResponseHandler(_gameEventWrapper.MarketController));
        //builder.AddResponseHandler(new GetCharacterEquipmentResponseHandler(_gameEventWrapper));
        //builder.AddResponseHandler(new FishingFinishResponseHandler(_gameEventWrapper.GatheringController));
        //builder.AddResponseHandler(new AuctionGetLoadoutOffersResponseHandler(_gameEventWrapper.MarketController));
        //builder.AddResponseHandler(new AuctionBuyLoadoutOfferResponseHandler(_gameEventWrapper.MarketController));
        //builder.AddResponseHandler(new GetGuildAccountLogsResponseHandler(_gameEventWrapper.GuildController));

        return builder.Build();
    }

    public void Start()
    {
        if (IsAnySocketActive())
        {
            return;
        }

        //ConsoleManager.WriteLineForMessage("Start Capture");

        _packetProvider.Start();

        //_ = App.ServiceProvider.GetService<SatNotificationManager>().ShowTrackingStatusAsync(LanguageController.Translation("START_TRACKING"), LanguageController.Translation("GAME_TRACKING_IS_STARTED"));
    }

    public void Stop()
    {
        if (!IsAnySocketActive())
        {
            return;
        }

        //ConsoleManager.WriteLineForMessage("Stop Capture");

        _packetProvider.Stop();

        //_ = App.ServiceProvider.GetService<SatNotificationManager>().ShowTrackingStatusAsync(LanguageController.Translation("STOP_TRACKING"), LanguageController.Translation("GAME_TRACKING_IS_STOPPED"));
    }

    private bool IsAnySocketActive()
    {
        return _packetProvider?.IsRunning ?? false;
    }
}