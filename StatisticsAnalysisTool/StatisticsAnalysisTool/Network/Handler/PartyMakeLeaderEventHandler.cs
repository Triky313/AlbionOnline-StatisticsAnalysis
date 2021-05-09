using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartyMakeLeaderEventHandler : ResponsePacketHandler<PartyMakeLeaderResponse>
    {
        private readonly TrackingController _trackingController;

        public PartyMakeLeaderEventHandler(TrackingController trackingController) : base((int) OperationCodes.PartyMakeLeader)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(PartyMakeLeaderResponse value)
        {
            _trackingController.AddDebugNotification(HandlerType.Operation, (int)OperationCodes.PartyMakeLeader, JsonConvert.SerializeObject(value));

            _trackingController.EntityController.RemoveFromParty(value.Username);
            await Task.CompletedTask;
        }
    }
}