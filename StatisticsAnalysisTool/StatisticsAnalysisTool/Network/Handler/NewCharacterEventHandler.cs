using System;
using System.Threading.Tasks;
using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;

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
            if (value.Guid != null && value.ObjectId != null)
                _trackingController.EntityController.AddEntity((long) value.ObjectId, (Guid) value.Guid, value.Name, GameObjectType.Player,
                    GameObjectSubType.Player);
            await Task.CompletedTask;
        }
    }
}