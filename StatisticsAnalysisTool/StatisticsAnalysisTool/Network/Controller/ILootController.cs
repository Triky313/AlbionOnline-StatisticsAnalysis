using StatisticsAnalysisTool.Models.NetworkModel;
using System;

namespace StatisticsAnalysisTool.Network.Controller
{
    public interface ILootController
    {
        public void AddDiscoveredLoot(DiscoveredLoot loot);
        public void AddPutLoot(long? objectId, Guid? playerGuid);
        public void ResetViewedLootLists();
    }
}