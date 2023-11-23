using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateMoneyEventHandler : EventPacketHandler<UpdateMoneyEvent>
{
    public UpdateMoneyEventHandler(ILiveStatsTracker liveStatsTracker) : base((int) EventCodes.UpdateMoney)
    {
    }

    protected override async Task OnActionAsync(UpdateMoneyEvent value)
    {
        await Task.CompletedTask;
    }
}