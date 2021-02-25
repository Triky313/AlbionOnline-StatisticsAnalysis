using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.ViewModels;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class JoinHandler : ResponsePacketHandler<JoinResponse>
    {
        private readonly TrackingController _trackingController;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public JoinHandler(TrackingController trackingController, MainWindowViewModel mainWindowViewModel) : base((int)OperationCodes.Join)
        {
            _trackingController = trackingController;
            _mainWindowViewModel = mainWindowViewModel;
        }

        protected override async Task OnActionAsync(JoinResponse value)
        {
            _mainWindowViewModel.TrackingUsername = value.Username;
            _mainWindowViewModel.TrackingGuildName = value.GuildName;
            _mainWindowViewModel.TrackingAllianceName = value.AllianceName;
            _mainWindowViewModel.TrackingCurrentMapName = value.UniqueMapName;

            _trackingController.SetTotalPlayerSilver(value.Silver.IntegerValue);
            _trackingController.CurrentPlayerUsername = value.Username;
            _trackingController.AddDungeon(value.MapType, value.DungeonGuid, value.MainMapIndex);

            ResetFameCounterByMapChangeIfActive();

            await Task.CompletedTask;
        }

        private void ResetFameCounterByMapChangeIfActive()
        {
            if (_mainWindowViewModel.IsTrackingResetByMapChangeActive)
            {
                _mainWindowViewModel.ResetMainCounters(true, true, true);
            }
        }
    }
}