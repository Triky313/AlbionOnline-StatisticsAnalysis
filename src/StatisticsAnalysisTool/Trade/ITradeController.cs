using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Trade;

public interface ITradeController
{
    Task AddTradeToBindingCollectionAsync(Trade trade);
    Task RemoveTradesByIdsAsync(IEnumerable<long> ids);
    Task RemoveTradesByDaysInSettingsAsync();
    void RegisterBuilding(long buildingObjectId);
    void UnregisterBuilding(long buildingObjectId);
    void AddCraftingBuildingInfo(CraftingBuildingInfo craftingBuildingInfo);
    void ResetCraftingBuildingInfo();
    void SetUpcomingTrade(long buildingObjectId, long dateTimeTicks, long internalTotalPrice, int quantity, int itemIndex);
    Task TradeFinishedAsync(long userObjectId, long buildingObjectId);
    Task LoadFromFileAsync();
    Task SaveInFileAsync();
    Task SaveInFileAfterExceedingLimit(int limit);
}