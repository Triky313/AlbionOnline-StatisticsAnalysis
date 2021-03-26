using System;
using System.Threading.Tasks;
using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class JoinResponseHandler : ResponsePacketHandler<JoinResponse>
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly TrackingController _trackingController;

        public JoinResponseHandler(TrackingController trackingController, MainWindowViewModel mainWindowViewModel) : base((int) OperationCodes.Join)
        {
            _trackingController = trackingController;
            _mainWindowViewModel = mainWindowViewModel;
        }

        protected override async Task OnActionAsync(JoinResponse value)
        {
            _trackingController.SetNewCluster(value.MapType, value.DungeonGuid, value.MapIndex, value.MainMapIndex);

            _trackingController.DungeonController.LocalUserData = new LocalUserData
            {
                UserObjectId = value.UserObjectId,
                Guid = value.Guid,
                Username = value.Username,
                LearningPoints = value.LearningPoints,
                Reputation = value.Reputation,
                ReSpecPoints = value.ReSpecPoints,
                Silver = value.Silver,
                Gold = value.Gold,
                GuildName = value.GuildName,
                MainMapIndex = value.MainMapIndex,
                PlayTimeInSeconds = value.PlayTimeInSeconds,
                AllianceName = value.AllianceName,
                CurrentDailyBonusPoints = value.CurrentDailyBonusPoints
            };

            _mainWindowViewModel.TrackingUsername = value.Username;
            _mainWindowViewModel.TrackingGuildName = value.GuildName;
            _mainWindowViewModel.TrackingAllianceName = value.AllianceName;
            _mainWindowViewModel.TrackingCurrentMapName = WorldData.GetUniqueNameOrDefault(value.MapIndex);

            if (value.Guid != null && value.UserObjectId != null)
            {
                _trackingController.EntityController.AddEntity((long) value.UserObjectId, (Guid) value.Guid, value.Username, GameObjectType.Player,
                    GameObjectSubType.LocalPlayer);
                _trackingController.EntityController.AddToParty((Guid) value.Guid, value.Username);
            }

            _trackingController.SetTotalPlayerSilver(value.Silver.IntegerValue);
            _trackingController.DungeonController?.AddDungeon(value.MapType, value.DungeonGuid, value.MainMapIndex);

            ResetFameCounterByMapChangeIfActive();

            await Task.CompletedTask;
        }

        private void ResetFameCounterByMapChangeIfActive()
        {
            if (_mainWindowViewModel.IsTrackingResetByMapChangeActive) _mainWindowViewModel.ResetMainCounters(true, true, true);
        }
    }
}