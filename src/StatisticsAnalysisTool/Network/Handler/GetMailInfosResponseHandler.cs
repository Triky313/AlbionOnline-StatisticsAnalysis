using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class GetMailInfosResponseHandler : ResponsePacketHandler<GetMailInfosResponse>
{
    private readonly TrackingController _trackingController;

    public GetMailInfosResponseHandler(TrackingController trackingController) : base((int) OperationCodes.GetMailInfos)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(GetMailInfosResponse value)
    {
        _trackingController.MailController.SetMailInfos(value.MailInfos);
        await Task.CompletedTask;
    }
}