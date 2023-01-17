using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class LeaveEventHandler
{
    private readonly TrackingController _trackingController;

    public LeaveEventHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(LeaveEvent value)
    {
        //if (value.ObjectId != null)
        //{
        //    TrackingController.EntityController.RemoveEntity((long)value.ObjectId);
        //}
        await Task.CompletedTask;
    }
}