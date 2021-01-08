using Albion.Network;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFameEventHandler : EventPacketHandler<UpdateFameEvent>
    {
        public UpdateFameEventHandler() : base(EventCodes.UpdateFame) { }

        protected override async Task OnActionAsync(UpdateFameEvent value)
        {
            //Debug.Print($"-----------------------------------------");
            //Debug.Print($"UpdateFame");
            //Debug.Print($"TotalFame: {value.TotalFame}");
            //Debug.Print($"FameWithZoneAndWithoutPremium: {value.FameWithZoneAndWithoutPremium}");
            //Debug.Print($"ZoneMultiplier: {value.ZoneMultiplier}");
            //Debug.Print($"NormalFame: {value.NormalFame}");
            //Debug.Print($"FameWithZoneAndPremium: {value.FameWithZoneAndPremium}");
            //Debug.Print($"PremiumFame: {value.PremiumFame}");
            //Debug.Print($"ZoneFame: {value.ZoneFame}");
            
            EventCounter(value.FameWithZoneAndPremium);
        }

        private double eventCounter;

        private void EventCounter(double fame)
        {
            eventCounter += fame;
            Debug.Print($"Fame counter: {eventCounter:N}");
        }
    }
}