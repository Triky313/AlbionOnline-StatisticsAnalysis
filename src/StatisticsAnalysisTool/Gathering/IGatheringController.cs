using System.Threading.Tasks;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.Gathering;

public interface IGatheringController
{
    Task AddOrUpdateAsync(HarvestFinishedObject harvestFinishedObject);
    void AddGatheredToBindingCollection(Gathered gathered);
    Task RemoveEntriesByAutoDeleteDateAsync();
    Task SetGatheredResourcesClosedAsync();
    void FishingIsStarted(long eventId, int itemIndex);
    void IsCurrentFishingSucceeded(bool isSucceeded);
    void CloseFishingEvent();
    void AddRewardItem(int itemIndex, int quantity);
    void AddFishedItem(DiscoveredItem item);
    Task FishingFinishedAsync();
    Task LoadFromFileAsync();
    Task SaveInFileAsync(bool safeMoreThan356Days = false);
    Task SaveInFileAfterExceedingLimit(int limit);
}