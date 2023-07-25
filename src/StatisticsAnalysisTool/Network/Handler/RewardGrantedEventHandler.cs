using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class RewardGrantedEventHandler : EventPacketHandler<RewardGrantedEvent>
{
    private readonly TrackingController _trackingController;

    public RewardGrantedEventHandler(TrackingController trackingController) : base((int) EventCodes.RewardGranted)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(RewardGrantedEvent value)
    {
        _trackingController.GatheringController.ConfirmReward(value.ItemIndex, value.Quantity);
        await Task.CompletedTask;
    }

}