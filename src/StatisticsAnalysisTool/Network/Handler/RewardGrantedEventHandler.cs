using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class RewardGrantedEventHandler : EventPacketHandler<RewardGrantedEvent>
{
    private readonly IGatheringController _gatheringController;

    public RewardGrantedEventHandler(IGatheringController gatheringController) : base((int) EventCodes.RewardGranted)
    {
        _gatheringController = gatheringController;
    }

    protected override async Task OnActionAsync(RewardGrantedEvent value)
    {
        _gatheringController.AddRewardItem(value.ItemIndex, value.Quantity);
        await Task.CompletedTask;
    }

}