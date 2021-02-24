using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class ChangeClusterResponseHandler : ResponsePacketHandler<ChangeClusterResponse>
    {
        public ChangeClusterResponseHandler() : base((int) OperationCodes.ChangeCluster) { }

        protected override async Task OnActionAsync(ChangeClusterResponse changeClusterResponse)
        {
            //TODO: Hanlder Manager implement
            await Task.CompletedTask;
        }
    }
}