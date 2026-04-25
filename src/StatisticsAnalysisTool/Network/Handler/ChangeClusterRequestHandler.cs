using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Operations.Request;

namespace StatisticsAnalysisTool.Network.Handler;

public class ChangeClusterRequestHandler(TrackingController trackingController) : RequestPacketHandler<ChangeClusterRequest>((int) OperationCodes.ChangeCluster)
{
    protected override Task OnActionAsync(ChangeClusterRequest value)
    {
        return Task.CompletedTask;
    }
}