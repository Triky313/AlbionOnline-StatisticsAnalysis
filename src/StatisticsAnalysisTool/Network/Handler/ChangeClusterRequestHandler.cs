using StatisticsAnalysisTool.Network.Operations.Request;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ChangeClusterRequestHandler() : RequestPacketHandler<ChangeClusterRequest>((int) OperationCodes.ChangeCluster)
{
    protected override Task OnActionAsync(ChangeClusterRequest value)
    {
        return Task.CompletedTask;
    }
}