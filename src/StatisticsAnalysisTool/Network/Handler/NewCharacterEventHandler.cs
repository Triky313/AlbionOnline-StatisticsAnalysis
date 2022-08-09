using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Events;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewCharacterEventHandler
    {
        private readonly TrackingController _trackingController;

        public NewCharacterEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(NewCharacterEvent value)
        {
            if (value.Guid != null && value.ObjectId != null)
            {
                _trackingController.EntityController.AddEntity((long) value.ObjectId, (Guid) value.Guid, null, 
                    value.Name, value.GuildName, string.Empty, GameObjectType.Player, GameObjectSubType.Player);
            }

            await Task.CompletedTask;
        }
    }
}