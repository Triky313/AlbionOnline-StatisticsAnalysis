using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Manager
{
    public interface ILootController
    {
        public Task AddLootAsync(Loot loot);
        public void AddDiscoveredLoot(DiscoveredLoot loot);
        public Task AddPutLootAsync(long? objectId, Guid? playerGuid);
        public void ResetViewedLootLists();
    }
}