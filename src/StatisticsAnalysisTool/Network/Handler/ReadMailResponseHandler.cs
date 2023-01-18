using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ReadMailResponseHandler : ResponsePacketHandler<ReadMailResponse>
{
    private readonly TrackingController _trackingController;

    public ReadMailResponseHandler(TrackingController trackingController) : base((int) OperationCodes.ReadMail)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(ReadMailResponse value)
    {
        await _trackingController.MailController.AddMailAsync(value.MailId, value.Content);
    }
}