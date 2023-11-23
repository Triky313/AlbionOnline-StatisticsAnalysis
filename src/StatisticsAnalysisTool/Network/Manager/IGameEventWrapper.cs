using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Guild;
using StatisticsAnalysisTool.PartyBuilder;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;

namespace StatisticsAnalysisTool.Network.Manager;

public interface IGameEventWrapper
{
    ITrackingController TrackingController { get; }
    ILootController LootController { get; }
    IEntityController EntityController { get; }
    IPartyBuilderController PartyBuilderController { get; }
    IClusterController ClusterController { get; }
    IDungeonController DungeonController { get; }
    ICombatController CombatController { get; }
    IStatisticController StatisticController { get; }
    ITreasureController TreasureController { get; }
    IMailController MailController { get; }
    IMarketController MarketController { get; }
    ITradeController TradeController { get; }
    IVaultController VaultController { get; }
    IGatheringController GatheringController { get; }
    IGuildController GuildController { get; }
    ILiveStatsTracker LiveStatsTracker { get; }
}