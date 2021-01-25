using Albion.Network;
using StatisticsAnalysisTool.ViewModels;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UserInformationHandler : ResponsePacketHandler<UserInformationEvent>
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        public UserInformationHandler(MainWindowViewModel mainWindowViewModel) : base(2)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        protected override async Task OnActionAsync(UserInformationEvent value)
        {
            _mainWindowViewModel.TrackingUsername = value.Username;
            _mainWindowViewModel.TrackingGuildName = value.GuildName;
            _mainWindowViewModel.TrackingAllianceName = value.AllianceName;
            _mainWindowViewModel.TrackingCurrentMapName = value.UniqueMapName;

            ResetFameCounterByMapChangeIfActive();

            await Task.CompletedTask;
        }

        private void ResetFameCounterByMapChangeIfActive()
        {
            if (_mainWindowViewModel.IsFameResetByMapChangeActive)
            {
                _mainWindowViewModel.ResetCounters(true, true, true);
            }
        }
    }
}