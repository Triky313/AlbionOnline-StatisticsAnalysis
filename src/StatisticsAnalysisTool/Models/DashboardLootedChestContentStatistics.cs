using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardLootedChestContentStatistics(DashboardContentType contentType) : BaseViewModel
{
    public DashboardContentType ContentType { get; } = contentType;
    public DashboardSummaryMetric TotalSummary { get; } = new();
    public string Name => LocalizationController.Translation(DashboardContentTypeResolver.GetTranslationKey(ContentType));

    public string ContentIconSource => ContentType switch
    {
        DashboardContentType.SoloDungeon => "/Assets/MiniMapMarker/solo_dungeon.png",
        DashboardContentType.StandardDungeon => "/Assets/MiniMapMarker/group_dungeon.png",
        DashboardContentType.AvalonDungeon => "/Assets/MiniMapMarker/raid_dungeon.png",
        DashboardContentType.StaticDungeon => "/Assets/MiniMapMarker/group_dungeon.png",
        DashboardContentType.AncientLands => "/Assets/ancient_lands_portal_small.png",
        DashboardContentType.AvalonianRoads => "/Assets/MiniMapMarker/road_chest_group.png",
        DashboardContentType.OpenWorld => string.Empty,
        DashboardContentType.HellGate => "/Assets/hellgate.png",
        DashboardContentType.CorruptedDungeon => "/Assets/currupted_dungeon.png",
        DashboardContentType.MistsDungeon => "/Assets/mists_dungeon.png",
        _ => "/Assets/dungeon.png"
    };

    public string ChestIconSource => ContentType == DashboardContentType.AvalonDungeon ? "/Assets/ava_chest.png" : "/Assets/static_chest.png";

    public int ChestIconSize => ContentType == DashboardContentType.AvalonDungeon ? 31 : 23;

    public int Common
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int Uncommon
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int Rare
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int Legendary
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double AveragePerMap
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public void Update(int total, int previousTotal, int common, int uncommon, int rare, int legendary, double averagePerMap)
    {
        TotalSummary.Update(total, total, previousTotal);
        Common = common;
        Uncommon = uncommon;
        Rare = rare;
        Legendary = legendary;
        AveragePerMap = averagePerMap;
    }

    public static string TranslationAverage => LocalizationController.Translation("AVERAGE");
    public static string TranslationMaps => LocalizationController.Translation("MAPS");
    public static string TranslationOpenedStandardChests => LocalizationController.Translation("OPENED_STANDARD_CHESTS");
    public static string TranslationOpenedUncommonChests => LocalizationController.Translation("OPENED_UNCOMMON_CHESTS");
    public static string TranslationOpenedRareChests => LocalizationController.Translation("OPENED_RARE_CHESTS");
    public static string TranslationOpenedLegendaryChests => LocalizationController.Translation("OPENED_LEGENDARY_CHESTS");
}
