using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ClaimPaymentTransactionRequestHandler : RequestPacketHandler<ClaimPaymentTransactionRequest>
{
    private readonly IEntityController _entityController;

    public ClaimPaymentTransactionRequestHandler(IEntityController entityController) : base((int) OperationCodes.ClaimPaymentTransaction)
    {
        _entityController = entityController;
    }

    protected override async Task OnActionAsync(ClaimPaymentTransactionRequest value)
    {
        _entityController.LocalUserData.IsReSpecActive = value.IsReSpecBoostActive;
        await Task.CompletedTask;
    }
}