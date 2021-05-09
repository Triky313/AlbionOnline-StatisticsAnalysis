using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Notification;
using System;
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
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.NewCharacter, JsonConvert.SerializeObject(value));

            if (value.Guid != null && value.ObjectId != null)
            {
                _trackingController.EntityController.AddEntity((long) value.ObjectId, (Guid) value.Guid, value.Name, GameObjectType.Player, GameObjectSubType.Player);
            }

            await Task.CompletedTask;
        }
    }
}