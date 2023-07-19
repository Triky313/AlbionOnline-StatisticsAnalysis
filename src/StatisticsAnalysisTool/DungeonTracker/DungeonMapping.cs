using StatisticsAnalysisTool.DungeonTracker.Models;

namespace StatisticsAnalysisTool.DungeonTracker;

public static class DungeonMapping
{
    public static DungeonDto Mapping(Dungeon dungeon)
    {
        return new DungeonDto()
        {
            TotalRunTimeInSeconds = dungeon.TotalRunTimeInSeconds,
            GuidList = dungeon.GuidList,
            EnterDungeonFirstTime = dungeon.EnterDungeonFirstTime,
            MainMapIndex = dungeon.MainMapIndex,
            DungeonEventObjects = dungeon.DungeonEventObjects,
            DungeonLoot = dungeon.DungeonLoot,
            Status = dungeon.Status,
            Fame = dungeon.Fame,
            ReSpec = dungeon.ReSpec,
            Silver = dungeon.Silver,
            Might = dungeon.Might,
            Favor = dungeon.Favor,
            FactionCoins = dungeon.FactionCoins,
            FactionFlags = dungeon.FactionFlags,
            DiedName = dungeon.DiedName,
            KilledBy = dungeon.KilledBy,
            DiedInDungeon = dungeon.DiedInDungeon,
            Faction = dungeon.Faction,
            Mode = dungeon.Mode,
            CityFaction = dungeon.CityFaction,
            Tier = dungeon.Tier,
            Level = dungeon.Level
        };
    }

    public static Dungeon Mapping(DungeonDto dungeonDto)
    {
        return new Dungeon()
        {
            TotalRunTimeInSeconds = dungeonDto.TotalRunTimeInSeconds,
            GuidList = dungeonDto.GuidList,
            EnterDungeonFirstTime = dungeonDto.EnterDungeonFirstTime,
            MainMapIndex = dungeonDto.MainMapIndex,
            DungeonEventObjects = dungeonDto.DungeonEventObjects,
            DungeonLoot = dungeonDto.DungeonLoot,
            Status = dungeonDto.Status,
            Fame = dungeonDto.Fame,
            ReSpec = dungeonDto.ReSpec,
            Silver = dungeonDto.Silver,
            Might = dungeonDto.Might,
            Favor = dungeonDto.Favor,
            FactionCoins = dungeonDto.FactionCoins,
            FactionFlags = dungeonDto.FactionFlags,
            DiedName = dungeonDto.DiedName,
            KilledBy = dungeonDto.KilledBy,
            DiedInDungeon = dungeonDto.DiedInDungeon,
            Faction = dungeonDto.Faction,
            Mode = dungeonDto.Mode,
            CityFaction = dungeonDto.CityFaction,
            Tier = dungeonDto.Tier,
            Level = dungeonDto.Level
        };
    }
}