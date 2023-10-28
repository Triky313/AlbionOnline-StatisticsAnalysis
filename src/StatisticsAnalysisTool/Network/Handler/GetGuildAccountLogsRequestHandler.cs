using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class GetGuildAccountLogsRequestHandler : RequestPacketHandler<GetGuildAccountLogsRequest>
{
    private readonly TrackingController _trackingController;

    public GetGuildAccountLogsRequestHandler(TrackingController trackingController) : base((int) OperationCodes.GetGuildAccountLogs)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(GetGuildAccountLogsRequest value)
    {
        _trackingController.GuildController.SetTabId(value.TabId);
        await Task.CompletedTask;
    }
}