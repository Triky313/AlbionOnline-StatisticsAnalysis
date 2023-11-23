using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class UseShrineRequestHandler : RequestPacketHandler<UseShrineRequest>
{
    public UseShrineRequestHandler() : base((int) OperationCodes.UseShrine)
    {
    }

    protected override async Task OnActionAsync(UseShrineRequest value)
    {
        await Task.CompletedTask;
    }
}