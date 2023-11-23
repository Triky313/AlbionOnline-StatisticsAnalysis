using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Manager;

public interface ITreasureController
{
    void RegisterEvents();
    void UnregisterEvents();
    void AddTreasure(int objectId, string uniqueName, string uniqueNameWithLocation);
    void UpdateTreasure(int objectId, List<Guid> openedBy);
    void RemoveTemporaryTreasures();
    void UpdateLootedChestsDashboardUi();
    Task LoadFromFileAsync();
    Task SaveInFileAsync();
}