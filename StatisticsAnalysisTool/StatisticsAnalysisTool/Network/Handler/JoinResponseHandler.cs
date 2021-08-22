using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

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
                InteractGuid = value.InteractGuid,
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
            };

            _mainWindowViewModel.TrackingUsername = value.Username;
            _mainWindowViewModel.TrackingGuildName = value.GuildName;
            _mainWindowViewModel.TrackingAllianceName = value.AllianceName;
            _mainWindowViewModel.TrackingCurrentMapName = WorldData.GetUniqueNameOrDefault(value.MapIndex);

            _mainWindowViewModel.DungeonCloseTimer = new DungeonCloseTimer
            {
                IsVisible = Visibility.Hidden
            };

            if (value.Guid != null && value.InteractGuid  != null && value.UserObjectId != null)
            {
                _trackingController.EntityController.AddEntity((long) value.UserObjectId, (Guid) value.Guid, (Guid) value.InteractGuid, value.Username, GameObjectType.Player, GameObjectSubType.LocalPlayer);
                _trackingController.EntityController.AddToParty((Guid) value.Guid, value.Username);
            }

            _trackingController.DungeonController?.AddDungeonAsync(value.MapType, value.DungeonGuid, value.MainMapIndex).ConfigureAwait(false);

            ResetFameCounterByMapChangeIfActive();
            SetTrackingIconColor();

            await Task.CompletedTask;
        }

        private void SetTrackingIconColor()
        {
            if (_trackingController.ExistIndispensableInfos)
            {
                _mainWindowViewModel.TrackingIconColor = TrackingIconType.On;
            }
        }

        private void ResetFameCounterByMapChangeIfActive()
        {
            if (_mainWindowViewModel.IsTrackingResetByMapChangeActive)
            {
                _mainWindowViewModel.ResetMainCounters();
            }
        }
    }
}