using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UserInformationHandler : ResponsePacketHandler<UserInformationEvent>
    {
        private readonly TrackingController _trackingController;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public UserInformationHandler(TrackingController trackingController, MainWindowViewModel mainWindowViewModel) : base(2)
        {
            _trackingController = trackingController;
            _mainWindowViewModel = mainWindowViewModel;
        }

        protected override async Task OnActionAsync(UserInformationEvent value)
        {
            _mainWindowViewModel.TrackingUsername = value.Username;
            _mainWindowViewModel.TrackingGuildName = value.GuildName;
            _mainWindowViewModel.TrackingAllianceName = value.AllianceName;
            _mainWindowViewModel.TrackingCurrentMapName = value.UniqueMapName;

            _trackingController.SetTotalPlayerSilver(value.Silver);

            _trackingController.AddDungeon(value.MapType, value.DungeonGuid);

            ResetFameCounterByMapChangeIfActive();

            await Task.CompletedTask;
        }

        private void ResetFameCounterByMapChangeIfActive()
        {
            if (_mainWindowViewModel.IsTrackingResetByMapChangeActive)
            {
                _mainWindowViewModel.ResetCounters(true, true, true);
            }
        }
    }
}