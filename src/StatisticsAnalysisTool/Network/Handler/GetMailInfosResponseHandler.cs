using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class GetMailInfosResponseHandler
{
    private readonly TrackingController _trackingController;

    public GetMailInfosResponseHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(GetMailInfosResponse value)
    {
        _trackingController.MailController.SetMailInfos(value.MailInfos);
        await Task.CompletedTask;
    }
}