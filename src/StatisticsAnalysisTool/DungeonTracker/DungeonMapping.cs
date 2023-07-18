using StatisticsAnalysisTool.DungeonTracker.Models;

namespace StatisticsAnalysisTool.DungeonTracker;

public static class DungeonMapping
{
    public static DungeonDto Mapping(Dungeon dungeonObjectOld)
    {
        return new DungeonDto()
        {
            TotalRunTimeInSeconds = dungeonObjectOld.TotalRunTimeInSeconds,
            GuidList = dungeonObjectOld.GuidList,
            EnterDungeonFirstTime = dungeonObjectOld.EnterDungeonFirstTime,
            MainMapIndex = dungeonObjectOld.MainMapIndex,
            DungeonEventObjects = dungeonObjectOld.DungeonEventObjects,
            DungeonLoot = dungeonObjectOld.DungeonLoot,
            Status = dungeonObjectOld.Status,
            Fame = dungeonObjectOld.Fame,
            ReSpec = dungeonObjectOld.ReSpec,
            Silver = dungeonObjectOld.Silver,
            Might = dungeonObjectOld.Might,
            Favor = dungeonObjectOld.Favor,
            FactionCoins = dungeonObjectOld.FactionCoins,
            FactionFlags = dungeonObjectOld.FactionFlags,
            DiedName = dungeonObjectOld.DiedName,
            KilledBy = dungeonObjectOld.KilledBy,
            DiedInDungeon = dungeonObjectOld.DiedInDungeon,
            Faction = dungeonObjectOld.Faction,
            Mode = dungeonObjectOld.Mode,
            CityFaction = dungeonObjectOld.CityFaction,
            Tier = dungeonObjectOld.Tier,
            Level = dungeonObjectOld.Level
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