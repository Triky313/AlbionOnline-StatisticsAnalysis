using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewExpeditionCheckPointHandler : EventPacketHandler<NewExpeditionCheckPointEvent>
{
    private readonly IDungeonController _dungeonController;

    public NewExpeditionCheckPointHandler(IDungeonController dungeonController) : base((int) EventCodes.NewExpeditionCheckPoint)
    {
        _dungeonController = dungeonController;
    }

    protected override async Task OnActionAsync(NewExpeditionCheckPointEvent value)
    {
        await _dungeonController.UpdateCheckPointAsync(new CheckPoint() { Id = value.ObjectId, Status = value.Status });
    }
}