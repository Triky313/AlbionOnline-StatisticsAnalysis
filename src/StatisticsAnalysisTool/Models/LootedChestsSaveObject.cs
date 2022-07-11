namespace StatisticsAnalysisTool.Models;

public class LootedChestsSaveObject
{
    public LootedChestsSaveObject(LootedChests lootedChests)
    {
        OpenWorldCommonWeek = lootedChests.OpenWorldCommonWeek;
        OpenWorldCommonMonth = lootedChests.OpenWorldCommonMonth;
        OpenWorldUncommonWeek = lootedChests.OpenWorldUncommonWeek;
        OpenWorldUncommonMonth = lootedChests.OpenWorldUncommonMonth;
        OpenWorldEpicWeek = lootedChests.OpenWorldEpicWeek;
        OpenWorldEpicMonth = lootedChests.OpenWorldEpicMonth;
        OpenWorldLegendaryWeek = lootedChests.OpenWorldLegendaryWeek;
        OpenWorldLegendaryMonth = lootedChests.OpenWorldLegendaryMonth;

        StaticCommonWeek = lootedChests.StaticCommonWeek;
        StaticCommonMonth = lootedChests.StaticCommonMonth;
        StaticUncommonWeek = lootedChests.StaticUncommonWeek;
        StaticUncommonMonth = lootedChests.StaticUncommonMonth;
        StaticEpicWeek = lootedChests.StaticEpicWeek;
        StaticEpicMonth = lootedChests.StaticEpicMonth;
        StaticLegendaryWeek = lootedChests.StaticLegendaryWeek;
        StaticLegendaryMonth = lootedChests.StaticLegendaryMonth;

        AvalonianRoadCommonWeek = lootedChests.AvalonianRoadCommonWeek;
        AvalonianRoadCommonMonth = lootedChests.AvalonianRoadCommonMonth;
        AvalonianRoadUncommonWeek = lootedChests.AvalonianRoadUncommonWeek;
        AvalonianRoadUncommonMonth = lootedChests.AvalonianRoadUncommonMonth;
        AvalonianRoadEpicWeek = lootedChests.AvalonianRoadEpicWeek;
        AvalonianRoadEpicMonth = lootedChests.AvalonianRoadEpicMonth;
        AvalonianRoadLegendaryWeek = lootedChests.AvalonianRoadLegendaryWeek;
        AvalonianRoadLegendaryMonth = lootedChests.AvalonianRoadLegendaryMonth;

        HellGateCommonWeek = lootedChests.HellGateCommonWeek;
        HellGateCommonMonth = lootedChests.HellGateCommonMonth;
        HellGateUncommonWeek = lootedChests.HellGateUncommonWeek;
        HellGateUncommonMonth = lootedChests.HellGateUncommonMonth;
        HellGateEpicWeek = lootedChests.HellGateEpicWeek;
        HellGateEpicMonth = lootedChests.HellGateEpicMonth;
        HellGateLegendaryWeek = lootedChests.HellGateLegendaryWeek;
        HellGateLegendaryMonth = lootedChests.HellGateLegendaryMonth;
    }

    #region OpenWorld bindings

    public int OpenWorldCommonWeek { get; set; }
    public int OpenWorldCommonMonth { get; set; }
    public int OpenWorldUncommonWeek { get; set; }
    public int OpenWorldUncommonMonth { get; set; }
    public int OpenWorldEpicWeek { get; set; }
    public int OpenWorldEpicMonth { get; set; }
    public int OpenWorldLegendaryWeek { get; set; }
    public int OpenWorldLegendaryMonth { get; set; }

    #endregion

    #region Static bindings

    public int StaticCommonWeek { get; set; }
    public int StaticCommonMonth { get; set; }
    public int StaticUncommonWeek { get; set; }
    public int StaticUncommonMonth { get; set; }
    public int StaticEpicWeek { get; set; }
    public int StaticEpicMonth { get; set; }
    public int StaticLegendaryWeek { get; set; }
    public int StaticLegendaryMonth { get; set; }

    #endregion

    #region Avalonian Road bindings

    public int AvalonianRoadCommonWeek { get; set; }
    public int AvalonianRoadCommonMonth { get; set; }
    public int AvalonianRoadUncommonWeek { get; set; }
    public int AvalonianRoadUncommonMonth { get; set; }
    public int AvalonianRoadEpicWeek { get; set; }
    public int AvalonianRoadEpicMonth { get; set; }
    public int AvalonianRoadLegendaryWeek { get; set; }
    public int AvalonianRoadLegendaryMonth { get; set; }

    #endregion

    #region Hellgate bindings

    public int HellGateCommonWeek { get; set; }
    public int HellGateCommonMonth { get; set; }
    public int HellGateUncommonWeek { get; set; }
    public int HellGateUncommonMonth { get; set; }
    public int HellGateEpicWeek { get; set; }
    public int HellGateEpicMonth { get; set; }
    public int HellGateLegendaryWeek { get; set; }
    public int HellGateLegendaryMonth { get; set; }

    #endregion
}