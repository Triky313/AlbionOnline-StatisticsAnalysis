using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class LogoutCancelRequestHandler(TrackingController trackingController) : RequestPacketHandler<LogoutCancelRequest>((int) OperationCodes.LogoutCancel)
{
    protected override Task OnActionAsync(LogoutCancelRequest value)
    {
        trackingController.CancelLogoutDetection();
        return Task.CompletedTask;
    }
}