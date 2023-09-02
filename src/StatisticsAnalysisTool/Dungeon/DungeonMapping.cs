using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.Enumerations;
using System.Linq;

namespace StatisticsAnalysisTool.Dungeon;

public class DungeonMapping
{
    public static DungeonDto Mapping(DungeonBaseFragment dungeon)
    {
        var dto = new DungeonDto()
        {
            Mode = dungeon.Mode,
            MapType = dungeon.MapType,
            MainMapIndex = dungeon.MainMapIndex,
            GuidList = dungeon.GuidList.ToList(),
            Tier = dungeon.Tier,
            Faction = dungeon.Faction,
            EnterDungeonFirstTime = dungeon.EnterDungeonFirstTime,
            TotalRunTimeInSeconds = dungeon.TotalRunTimeInSeconds,
            Events = dungeon.Events.Select(Mapping).ToList(),
            Loot = dungeon.Loot.Select(Mapping).ToList(),
            Status = DungeonStatus.Done,
            Fame = dungeon.Fame,
            ReSpec = dungeon.ReSpec,
            Silver = dungeon.Silver,
            DiedName = dungeon.DiedName,
            KilledBy = dungeon.KilledBy,
            KillStatus = dungeon.KillStatus
        };

        if (dungeon is RandomDungeonFragment randomDungeon)
        {
            dto.Level = randomDungeon.Level;
            dto.CityFaction = randomDungeon.CityFaction;
            dto.FactionCoins = randomDungeon.FactionCoins;
            dto.FactionFlags = randomDungeon.FactionFlags;
            dto.Might = randomDungeon.Might;
            dto.Favor = randomDungeon.Favor;
        }

        if (dungeon is CorruptedFragment corrupted)
        {
            dto.Might = corrupted.Might;
            dto.Favor = corrupted.Favor;
        }

        if (dungeon is ExpeditionFragment expedition)
        {
            dto.CheckPoints = expedition.CheckPoints.Select(Mapping).ToList();
        }

        if (dungeon is HellGateFragment hellGate)
        {
            dto.Might = hellGate.Might;
            dto.Favor = hellGate.Favor;
        }

        if (dungeon is MistsFragment mists)
        {
            dto.Might = mists.Might;
            dto.Favor = mists.Favor;
            dto.MistsRarity = mists.Rarity;
            dto.BrecilianStanding = mists.BrecilianStanding;
        }

        if (dungeon is MistsDungeonFragment mistsDungeon)
        {
            dto.Might = mistsDungeon.Might;
            dto.Favor = mistsDungeon.Favor;
        }

        return dto;
    }

    public static DungeonBaseFragment Mapping(DungeonDto dto)
    {
        return dto.Mode switch
        {
            DungeonMode.Solo or DungeonMode.Standard => new RandomDungeonFragment(dto),
            DungeonMode.Corrupted => new CorruptedFragment(dto),
            DungeonMode.HellGate => new HellGateFragment(dto),
            DungeonMode.Expedition => new ExpeditionFragment(dto),
            DungeonMode.Mists => new MistsFragment(dto),
            DungeonMode.MistsDungeon => new MistsDungeonFragment(dto),
            _ => null
        };
    }

    public static PointOfInterest Mapping(DungeonEventDto dto)
    {
        return new PointOfInterest()
        {
            Id = dto.Id,
            IsBossChest = dto.IsBossChest,
            Opened = dto.Opened,
            Rarity = dto.Rarity,
            ShrineBuff = dto.ShrineBuff,
            ShrineType = dto.ShrineType,
            Status = dto.Status,
            Type = dto.Type,
            UniqueName = dto.UniqueName
        };
    }

    public static DungeonEventDto Mapping(PointOfInterest pointOfInterest)
    {
        return new DungeonEventDto()
        {
            Id = pointOfInterest.Id,
            IsBossChest = pointOfInterest.IsBossChest,
            Opened = pointOfInterest.Opened,
            Rarity = pointOfInterest.Rarity,
            ShrineBuff = pointOfInterest.ShrineBuff,
            ShrineType = pointOfInterest.ShrineType,
            Status = pointOfInterest.Status,
            Type = pointOfInterest.Type,
            UniqueName = pointOfInterest.UniqueName
        };
    }

    public static Loot Mapping(LootDto dto)
    {
        return new Loot()
        {
            UniqueName = dto.UniqueName,
            UtcDiscoveryTime = dto.UtcDiscoveryTime,
            Quantity = dto.Quantity,
            EstimatedMarketValueInternal = dto.EstimatedMarketValueInternal
        };
    }

    public static LootDto Mapping(Loot loot)
    {
        return new LootDto()
        {
            UniqueName = loot.UniqueName,
            UtcDiscoveryTime = loot.UtcDiscoveryTime,
            Quantity = loot.Quantity,
            EstimatedMarketValueInternal = loot.EstimatedMarketValueInternal
        };
    }

    public static CheckPoint Mapping(CheckPointDto dto)
    {
        return new CheckPoint()
        {
            Status = dto.Status
        };
    }

    public static CheckPointDto Mapping(CheckPoint checkPoint)
    {
        return new CheckPointDto()
        {
            Status = checkPoint.Status
        };
    }
}