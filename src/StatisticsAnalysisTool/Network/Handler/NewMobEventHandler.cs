using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewMobEventHandler : EventPacketHandler<NewMobEvent>
{
    private readonly IDungeonController _dungeonController;

    public NewMobEventHandler(IDungeonController dungeonController) : base((int) EventCodes.NewMob)
    {
        _dungeonController = dungeonController;
    }

    protected override async Task OnActionAsync(NewMobEvent value)
    {
        await _dungeonController.AddTierToCurrentDungeonAsync(value.MobIndex);
        _dungeonController.AddLevelToCurrentDungeon(value.MobIndex, value.HitPointsMax);
    }
}