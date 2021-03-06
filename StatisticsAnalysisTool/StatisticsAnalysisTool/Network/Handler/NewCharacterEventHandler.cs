using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewCharacterEventHandler : EventPacketHandler<NewCharacterEvent>
    {
        private readonly TrackingController _trackingController;
        public NewCharacterEventHandler(TrackingController trackingController) : base((int) EventCodes.NewCharacter)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(NewCharacterEvent value)
        {
            if (value.ObjectId != null)
            {
                _trackingController.EntityController.AddEntity((long)value.ObjectId, value.Guid, value.Name, GameObjectType.Player, GameObjectSubType.Player);
            }
            await Task.CompletedTask;
        }
    }
}