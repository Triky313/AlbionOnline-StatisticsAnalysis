using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Guild;
using StatisticsAnalysisTool.PartyBuilder;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;

namespace StatisticsAnalysisTool.Network.Manager;

public class GameEventWrapper : IGameEventWrapper
{
    public ITrackingController TrackingController { get; }
    public ILootController LootController { get; }
    public IEntityController EntityController { get; }
    public IPartyBuilderController PartyBuilderController { get; }
    public IClusterController ClusterController { get; }
    public IDungeonController DungeonController { get; }
    public ICombatController CombatController { get; }
    public IStatisticController StatisticController { get; }
    public ITreasureController TreasureController { get; }
    public IMailController MailController { get; }
    public IMarketController MarketController { get; }
    public ITradeController TradeController { get; }
    public IVaultController VaultController { get; }
    public IGatheringController GatheringController { get; }
    public IGuildController GuildController { get; }
    public ILiveStatsTracker LiveStatsTracker { get; }

    public GameEventWrapper(
        ITrackingController trackingController,
        ILootController lootController,
        IEntityController entityController,
        IPartyBuilderController partyBuilderController,
        IClusterController clusterController,
        IDungeonController dungeonController,
        ICombatController combatController,
        IStatisticController statisticController,
        ITreasureController treasureController,
        IMailController mailController,
        IMarketController marketController,
        ITradeController tradeController,
        IVaultController vaultController,
        IGatheringController gatheringController,
        IGuildController guildController,
        ILiveStatsTracker liveStatsTracker)
    {
        TrackingController = trackingController;
        LootController = lootController;
        EntityController = entityController;
        PartyBuilderController = partyBuilderController;
        ClusterController = clusterController;
        DungeonController = dungeonController;
        CombatController = combatController;
        StatisticController = statisticController;
        TreasureController = treasureController;
        MailController = mailController;
        MarketController = marketController;
        TradeController = tradeController;
        VaultController = vaultController;
        GatheringController = gatheringController;
        GuildController = guildController;
        LiveStatsTracker = liveStatsTracker;
    }
}