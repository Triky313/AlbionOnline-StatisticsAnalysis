using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;
using SkiaSharp;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.Nats;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class AuctionGetOffersResponseHandler
    {
        private readonly TrackingController _trackingController;

        public AuctionGetOffersResponseHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(AuctionGetOffersResponse value)
        {
            await PushMarketOrderAsync(value.AuctionGetOffers);
        }

        private static async Task PushMarketOrderAsync(IEnumerable<AuctionGetOffer> auctionOffers)
        {
            var clusterIndex = ClusterController.CurrentCluster.Index;

            if (string.IsNullOrEmpty(clusterIndex))
            {
                return;
            }

            if (!int.TryParse(clusterIndex, NumberStyles.Any, CultureInfo.CurrentCulture, out var intIndex) || intIndex == 0)
            {
                return;
            }
            
            foreach (var valueAuctionGetOffer in auctionOffers)
            {
                await NatsController.PushMarketOrderAsync(new NatsMarketOrder(valueAuctionGetOffer, intIndex));
            }
        }
    }
}