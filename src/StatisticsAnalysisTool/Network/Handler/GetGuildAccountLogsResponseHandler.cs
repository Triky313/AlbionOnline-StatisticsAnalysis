using StatisticsAnalysisTool.Guild;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class GetGuildAccountLogsResponseHandler : ResponsePacketHandler<GetGuildAccountLogsResponse>
{
    private readonly IGuildController _guildController;

    public GetGuildAccountLogsResponseHandler(IGuildController guildController) : base((int) OperationCodes.GetGuildAccountLogs)
    {
        _guildController = guildController;
    }

    protected override async Task OnActionAsync(GetGuildAccountLogsResponse value)
    {
        _guildController.AddSiphonedEnergyEntries(value.Usernames, value.Quantities, value.Timestamps);
        await Task.CompletedTask;
    }
}