using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateMistCityStandingEventHandler : EventPacketHandler<UpdateMistCityStandingEvent>
{
    private readonly IDungeonController _dungeonController;

    public UpdateMistCityStandingEventHandler(IDungeonController dungeonController) : base((int) EventCodes.UpdateMistCityStanding)
    {
        _dungeonController = dungeonController;
    }

    protected override async Task OnActionAsync(UpdateMistCityStandingEvent value)
    {
        _dungeonController?.AddValueToDungeon(value.TotalPoints.DoubleValue, ValueType.BrecilianStanding);
        await Task.CompletedTask;
    }
}