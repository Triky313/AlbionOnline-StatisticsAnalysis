using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class ActiveSpellEffectsUpdateEventHandler : EventPacketHandler<ActiveSpellEffectsUpdateEvent>
    {
        private readonly TrackingController _trackingController;

        public ActiveSpellEffectsUpdateEventHandler(TrackingController trackingController) : base((int) EventCodes.ActiveSpellEffectsUpdate)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(ActiveSpellEffectsUpdateEvent value)
        {
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.ActiveSpellEffectsUpdate, JsonConvert.SerializeObject(value));

            if (value.CauserId != null)
            {
                var spellEffect = new SpellEffect
                {
                    CauserId = value.CauserId.ObjectToLong() ?? 0,
                    SpellIndex = value.SpellIndex
                };

                _trackingController.EntityController.AddSpellEffect(spellEffect);
            }

            await Task.CompletedTask;
        }
    }
}