using System.Collections.Generic;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Guild;

public interface IGuildController
{
    void SetTabId(int id);
    void AddSiphonedEnergyEntry(string username, FixPoint quantity, long timestamp, bool isManualEntry = false);
    void AddSiphonedEnergyEntries(List<string> usernames, List<FixPoint> quantities, List<long> timestamps, bool isManualEntry = false);
    void UpdateSiphonedEnergyOverview();
    Task RemoveTradesByIdsAsync(IEnumerable<int> hashCodes);
    Task LoadFromFileAsync();
    Task SaveInFileAsync();
}