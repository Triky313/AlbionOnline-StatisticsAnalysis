using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Dungeon;

public interface IDungeonController
{
    Task AddDungeonAsync(MapType mapType, Guid? mapGuid);
    void ResetDungeons();
    void ResetDungeonsByDateAscending(DateTime date);
    void DeleteDungeonsWithZeroFame();
    void RemoveDungeon(string dungeonHash);
    Task RemoveDungeonByHashAsync(IEnumerable<string> dungeonHash);

    void SetDungeonChestOpen(int id, List<Guid> allowedToOpen);
    Task SetDungeonEventInformationAsync(int id, string uniqueName);
    void AddValueToDungeon(double value, ValueType valueType, CityFaction cityFaction = CityFaction.Unknown);
    void SetDiedIfInDungeon(DiedObject dieObject);

    void AddLevelToCurrentDungeon(int? mobIndex, double hitPointsMax);
    Task AddTierToCurrentDungeonAsync(int? mobIndex);

    void SetCurrentItemContainer(ItemContainerObject itemContainerObject);
    void AddDiscoveredItem(DiscoveredItem discoveredItem);
    Task AddNewLocalPlayerLootOnCurrentDungeonAsync(int containerSlot, Guid containerGuid, Guid userInteractGuid);
    Task AddLocalPlayerLootedItemToCurrentDungeonAsync(DiscoveredItem discoveredItem);
    public void ResetLocalPlayerDiscoveredLoot();

    Task UpdateCheckPointAsync(CheckPoint checkPoint);

    Task LoadDungeonFromFileAsync();
    Task SaveInFileAsync();
}