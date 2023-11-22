using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class GetGuildAccountLogsResponseHandler : ResponsePacketHandler<GetGuildAccountLogsResponse>
{
    private readonly TrackingController _trackingController;

    public GetGuildAccountLogsResponseHandler(TrackingController trackingController) : base((int) OperationCodes.GetGuildAccountLogs)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(GetGuildAccountLogsResponse value)
    {
        _trackingController.GuildController.AddSiphonedEnergyEntries(value.Usernames, value.Quantities, value.Timestamps);
        await Task.CompletedTask;
    }
}