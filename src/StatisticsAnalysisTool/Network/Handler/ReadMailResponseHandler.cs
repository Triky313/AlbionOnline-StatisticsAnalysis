using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ReadMailResponseHandler(TrackingController trackingController) : ResponsePacketHandler<ReadMailResponse>((int) OperationCodes.ReadMail)
{
    protected override async Task OnActionAsync(ReadMailResponse value)
    {
        await trackingController.MailController.AddMailAsync(value.MailId, value.Content);
    }
}