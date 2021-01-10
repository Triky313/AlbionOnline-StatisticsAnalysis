using Albion.Network;
using StatisticsAnalysisTool.Models.NetworkModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFameEventHandler : EventPacketHandler<UpdateFameEvent>
    {
        private readonly ObservableCollection<TrackingNotification> _trackingNotifications;

        public UpdateFameEventHandler(ObservableCollection<TrackingNotification> trackingNotifications) : base(EventCodes.UpdateFame)
        {
            _trackingNotifications = trackingNotifications;
        }

        protected override async Task OnActionAsync(UpdateFameEvent value)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                _trackingNotifications.Add(new TrackingNotification($"Gained Fame: {value.TotalGainedFame} ({value.NormalFame} Normal, {value.ZoneFame} Zone, {value.PremiumFame}Premium, {value.SatchelFame} Satchel)"));
            });

            EventCounter(value.TotalGainedFame);
            await Task.CompletedTask;
        }

        private double eventCounter;

        private void EventCounter(double fame)
        {
            eventCounter += fame;
            Debug.Print($"Fame counter: {eventCounter:N}");
        }
    }
}