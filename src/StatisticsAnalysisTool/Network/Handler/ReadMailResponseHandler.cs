using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.Trade.Mails;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ReadMailResponseHandler : ResponsePacketHandler<ReadMailResponse>
{
    private readonly IMailController _mailController;

    public ReadMailResponseHandler(IMailController mailController) : base((int) OperationCodes.ReadMail)
    {
        _mailController = mailController;
    }

    protected override async Task OnActionAsync(ReadMailResponse value)
    {
        await _mailController.AddMailAsync(value.MailId, value.Content);
    }
}