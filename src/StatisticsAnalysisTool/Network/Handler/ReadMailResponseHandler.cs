using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ReadMailResponseHandler
{
    private readonly TrackingController _trackingController;

    public ReadMailResponseHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(ReadMailResponse value)
    {
        await _trackingController.MailController.AddMailAsync(value.MailId, value.Content);
    }
}