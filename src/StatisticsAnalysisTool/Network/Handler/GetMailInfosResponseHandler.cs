using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.Trade.Mails;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class GetMailInfosResponseHandler : ResponsePacketHandler<GetMailInfosResponse>
{
    private readonly IMailController _mailController;

    public GetMailInfosResponseHandler(IMailController mailController) : base((int) OperationCodes.GetMailInfos)
    {
        _mailController = mailController;
    }

    protected override async Task OnActionAsync(GetMailInfosResponse value)
    {
        _mailController.SetMailInfos(value.MailInfos);
        await Task.CompletedTask;
    }
}