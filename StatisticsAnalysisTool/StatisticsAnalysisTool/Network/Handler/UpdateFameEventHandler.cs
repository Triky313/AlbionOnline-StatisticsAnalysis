using Albion.Network;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateFameEventHandler : EventPacketHandler<UpdateFameEvent>
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        public UpdateFameEventHandler(MainWindowViewModel mainWindowViewModel) : base(EventCodes.UpdateFame)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        protected override async Task OnActionAsync(UpdateFameEvent value)
        {
            _mainWindowViewModel.AddTrackingNotification(new TrackingNotification(
                $"Gained Fame: {value.TotalGainedFame} ({value.NormalFame} Normal, {value.ZoneFame} Zone, {value.PremiumFame}Premium, {value.SatchelFame} Satchel)"));

            //_mainWindow.Dispatcher.Invoke(delegate
            //{
            //    _trackingNotifications.Add(new TrackingNotification($"Gained Fame: {value.TotalGainedFame} ({value.NormalFame} Normal, {value.ZoneFame} Zone, {value.PremiumFame}Premium, {value.SatchelFame} Satchel)"));
            //});

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