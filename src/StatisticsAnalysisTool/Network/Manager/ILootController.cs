using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Network.Manager;

public interface ILootController
{
    void RegisterEvents();
    void UnregisterEvents();
    Task AddLootAsync(Loot loot);
    void ClearLootLogger();
    string GetLootLoggerObjectsAsCsv();
    void SetIdentifiedBody(long objectId, string lootBody);
    void SetCurrentItemContainer(ItemContainerObject itemContainerObject);
    void AddDiscoveredItem(DiscoveredItem discoveredItem);
    Task AddNewLocalPlayerLootAsync(int containerSlot, Guid containerGuid, Guid userInteractGuid);
    void ResetLocalPlayerDiscoveredLoot();
    void ResetIdentifiedBodies();
    Item GetItemFromDiscoveredLoot(long objectId);
}