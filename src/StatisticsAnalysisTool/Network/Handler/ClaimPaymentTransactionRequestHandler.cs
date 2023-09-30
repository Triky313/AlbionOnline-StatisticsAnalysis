using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Operations.Responses;

namespace StatisticsAnalysisTool.Network.Handler;

public class ClaimPaymentTransactionRequestHandler : RequestPacketHandler<ClaimPaymentTransactionRequest>
{
    private readonly TrackingController _trackingController;

    public ClaimPaymentTransactionRequestHandler(TrackingController trackingController) : base((int) OperationCodes.ClaimPaymentTransaction)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(ClaimPaymentTransactionRequest value)
    {
        _trackingController.EntityController.LocalUserData.IsReSpecActive = value.IsReSpecBoostActive;
        await Task.CompletedTask;
    }
}