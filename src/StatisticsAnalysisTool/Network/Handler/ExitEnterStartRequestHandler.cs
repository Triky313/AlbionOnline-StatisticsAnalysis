using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Request;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ExitEnterStartRequestHandler(TrackingController trackingController) : RequestPacketHandler<ExitEnterStartRequest>((int) OperationCodes.ExitEnterStart)
{
    protected override Task OnActionAsync(ExitEnterStartRequest value)
    {
        trackingController.DungeonController.SelectRandomDungeonExit(value.TargetObjectId);
        return Task.CompletedTask;
    }
}