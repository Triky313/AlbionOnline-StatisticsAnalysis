using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class PartyMakeLeaderResponseHandler : ResponsePacketHandler<PartyMakeLeaderResponse>
{
    public PartyMakeLeaderResponseHandler() : base((int) OperationCodes.PartyMakeLeader)
    {
    }

    protected override async Task OnActionAsync(PartyMakeLeaderResponse value)
    {
        await Task.CompletedTask;
    }
}