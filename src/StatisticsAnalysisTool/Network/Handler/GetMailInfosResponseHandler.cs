using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class GetMailInfosResponseHandler
    {
        private readonly TrackingController _trackingController;

        public GetMailInfosResponseHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(GetMailInfosResponse value)
        {
            //if (value.ObjectId != null)
            //{
            //    TrackingController.EntityController.RemoveEntity((long)value.ObjectId);
            //}
            await Task.CompletedTask;
        }
    }
}