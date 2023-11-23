using StatisticsAnalysisTool.Guild;
using StatisticsAnalysisTool.Network.Operations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class GetGuildAccountLogsRequestHandler : RequestPacketHandler<GetGuildAccountLogsRequest>
{
    private readonly IGuildController _guildController;

    public GetGuildAccountLogsRequestHandler(IGuildController guildController) : base((int) OperationCodes.GetGuildAccountLogs)
    {
        _guildController = guildController;
    }

    protected override async Task OnActionAsync(GetGuildAccountLogsRequest value)
    {
        _guildController.SetTabId(value.TabId);
        await Task.CompletedTask;
    }
}