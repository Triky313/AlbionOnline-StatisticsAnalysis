using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class AuctionGetOffersEventHandler : ResponsePacketHandler<AuctionGetOffersOperation>
    {
        private readonly TrackingController _trackingController;

        public AuctionGetOffersEventHandler(TrackingController trackingController) : base((int)OperationCodes.AuctionGetOffers)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(AuctionGetOffersOperation testEvent)
        {
            await Task.CompletedTask;
        }
    }
}