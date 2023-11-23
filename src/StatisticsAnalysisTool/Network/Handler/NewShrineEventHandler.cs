using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewShrineEventHandler : EventPacketHandler<NewShrineEvent>
{
    private readonly IDungeonController _dungeonController;

    public NewShrineEventHandler(IDungeonController dungeonController) : base((int) EventCodes.NewShrine)
    {
        _dungeonController = dungeonController;
    }

    protected override async Task OnActionAsync(NewShrineEvent value)
    {
        await _dungeonController?.SetDungeonEventInformationAsync(value.Id, value.UniqueName)!;
        await Task.CompletedTask;
    }
}