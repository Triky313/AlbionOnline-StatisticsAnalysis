using StatisticsAnalysisTool.Models.NetworkModel;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Manager;

public interface ILootController
{
    public Task AddLootAsync(Loot loot);
}