using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models;

public static class DashboardContentTypeResolver
{
    public static DashboardContentType Resolve(
        MapType mapType,
        DungeonMode dungeonMode,
        ClusterMode clusterMode)
    {
        return mapType switch
        {
            MapType.RandomDungeon when dungeonMode == DungeonMode.Solo => DashboardContentType.SoloDungeon,
            MapType.RandomDungeon when dungeonMode == DungeonMode.Standard => DashboardContentType.StandardDungeon,
            MapType.RandomDungeon when dungeonMode == DungeonMode.Avalon => DashboardContentType.AvalonDungeon,
            MapType.HellGate => DashboardContentType.HellGate,
            MapType.CorruptedDungeon => DashboardContentType.CorruptedDungeon,
            MapType.Expedition => DashboardContentType.Expedition,
            MapType.Mists => DashboardContentType.Mists,
            MapType.MistsDungeon => DashboardContentType.MistsDungeon,
            MapType.AbyssalDepths => DashboardContentType.AbyssalDepths,
            MapType.Unknown when IsOpenWorldCluster(clusterMode) => DashboardContentType.OpenWorld,
            MapType.Arena => DashboardContentType.Arena,
            _ => DashboardContentType.Others
        };
    }

    public static string GetTranslationKey(DashboardContentType contentType)
    {
        return contentType switch
        {
            DashboardContentType.SoloDungeon => "SOLO_DUNGEON",
            DashboardContentType.StandardDungeon => "STANDARD_DUNGEON",
            DashboardContentType.AvalonDungeon => "AVALONIAN_DUNGEON",
            DashboardContentType.HellGate => "HELLGATE",
            DashboardContentType.CorruptedDungeon => "CORRUPTED_DUNGEON",
            DashboardContentType.Expedition => "EXPEDITION",
            DashboardContentType.Mists => "MISTS",
            DashboardContentType.MistsDungeon => "MISTS_DUNGEON",
            DashboardContentType.AbyssalDepths => "ABYSSALDEPTHS",
            DashboardContentType.OpenWorld => "OPEN_WORLD",
            DashboardContentType.Arena => "ARENA",
            _ => "OTHERS"
        };
    }

    public static string GetBrushResourceKey(DashboardContentType contentType)
    {
        return contentType switch
        {
            DashboardContentType.SoloDungeon => "SolidColorBrush.Dungeon.Mode.Solo.1",
            DashboardContentType.StandardDungeon => "SolidColorBrush.Dungeon.Mode.Standard.1",
            DashboardContentType.AvalonDungeon => "SolidColorBrush.Dungeon.Mode.Avalon.1",
            DashboardContentType.HellGate => "SolidColorBrush.Dungeon.Mode.HellGate.1",
            DashboardContentType.CorruptedDungeon => "SolidColorBrush.Dungeon.Mode.Corrupted.1",
            DashboardContentType.Expedition => "SolidColorBrush.Dungeon.Mode.Expedition.1",
            DashboardContentType.Mists => "SolidColorBrush.Dungeon.Mode.Mists.1",
            DashboardContentType.MistsDungeon => "SolidColorBrush.Dungeon.Mode.MistsDungeon.1",
            DashboardContentType.AbyssalDepths => "SolidColorBrush.Dungeon.Mode.AbyssalDepths.1",
            DashboardContentType.OpenWorld => "SolidColorBrush.Content.OpenWorld",
            DashboardContentType.Arena => "SolidColorBrush.Content.Arena",
            _ => "SolidColorBrush.Content.Others"
        };
    }

    private static bool IsOpenWorldCluster(ClusterMode clusterMode)
    {
        return clusterMode is ClusterMode.SafeArea
            or ClusterMode.Yellow
            or ClusterMode.Red
            or ClusterMode.Black;
    }
}
