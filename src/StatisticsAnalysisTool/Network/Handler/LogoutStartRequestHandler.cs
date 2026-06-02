using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class LogoutStartRequestHandler(TrackingController trackingController) : RequestPacketHandler<LogoutStartRequest>((int) OperationCodes.LogoutStart)
{
    protected override Task OnActionAsync(LogoutStartRequest value)
    {
        trackingController.BeginLogoutDetection();
        return Task.CompletedTask;
    }
}