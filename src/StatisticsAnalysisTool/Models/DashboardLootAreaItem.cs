using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Dungeon;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardLootAreaItem
{
    public DashboardLootAreaItem(string name, long itemCount, double totalValue, ClusterType clusterType, DungeonMode dungeonMode)
    {
        Name = name;
        ItemCount = itemCount;
        TotalValue = totalValue;
        ClusterType = clusterType;
        DungeonMode = dungeonMode;
    }

    public string Name { get; }
    public long ItemCount { get; }
    public double TotalValue { get; }
    public ClusterType ClusterType { get; }
    public DungeonMode DungeonMode { get; }

    public string IconSource => DungeonMode switch
    {
        DungeonMode.HellGate => "/Assets/hellgate.png",
        DungeonMode.Corrupted => "/Assets/currupted_dungeon.png",
        DungeonMode.Expedition => "/Assets/dungeon.png",
        DungeonMode.Mists => "/Assets/shiny_common.png",
        DungeonMode.MistsDungeon => "/Assets/mists_dungeon.png",
        DungeonMode.AbyssalDepths => "/Assets/abyssal_depths.png",
        DungeonMode.DragonArea => string.Empty,
        _ => ClusterType switch
        {
            ClusterType.SafeArea => "/Assets/map_blue_icon.png",
            ClusterType.Yellow => "/Assets/map_yellow_icon.png",
            ClusterType.Red => "/Assets/map_red_icon.png",
            ClusterType.Black => "/Assets/map_black_icon.png",
            _ => "/Assets/map_white_icon.png"
        }
    };
}