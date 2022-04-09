using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class JoinResponseHandler
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly TrackingController _trackingController;

        public JoinResponseHandler(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;
            _mainWindowViewModel = mainWindowViewModel;
        }

        public async Task OnActionAsync(JoinResponse value)
        {
            await SetLocalUserData(value);

            _trackingController.SetNewCluster(value.MapType, value.DungeonGuid, value.MapIndex, value.MainMapIndex);

            _mainWindowViewModel.TrackingUsername = value.Username;
            _mainWindowViewModel.TrackingGuildName = value.GuildName;
            _mainWindowViewModel.TrackingAllianceName = value.AllianceName;
            _mainWindowViewModel.TrackingCurrentMapName = WorldData.GetUniqueNameOrDefault(value.MapIndex);

            SetCharacterTrackedVisibility(value.Username);

            _mainWindowViewModel.DungeonCloseTimer = new DungeonCloseTimer
            {
                IsVisible = Visibility.Collapsed
            };

            await AddEntityAsync(value.UserObjectId, value.Guid, value.InteractGuid, value.Username, GameObjectType.Player, GameObjectSubType.LocalPlayer);

            _trackingController.DungeonController?.AddDungeonAsync(value.MapType, value.DungeonGuid).ConfigureAwait(false);

            ResetFameCounterByMapChangeIfActive();
            SetTrackingActivityText();
        }

        private async Task SetLocalUserData(JoinResponse value)
        {
            await _trackingController.EntityController.LocalUserData.SetValuesAsync(new LocalUserData
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
                AllianceName = value.AllianceName
            });
        }

        private async Task AddEntityAsync(long? userObjectId, Guid? guid, Guid? interactGuid, string name, GameObjectType gameObjectType, GameObjectSubType gameObjectSubType)
        {
            if (guid == null || interactGuid == null || userObjectId == null)
            {
                return;
            }
            
            _trackingController.EntityController.AddEntity((long)userObjectId, (Guid)guid, interactGuid, name, GameObjectType.Player, GameObjectSubType.LocalPlayer);
            await _trackingController.EntityController.AddToPartyAsync((Guid)guid, name);
        }

        private void SetTrackingActivityText()
        {
            if (_trackingController.ExistIndispensableInfos)
            {
                _mainWindowViewModel.TrackingActiveText = MainWindowTranslation.TrackingIsActive;
                _mainWindowViewModel.TrackingActivityColor = TrackingIconType.On;
            }
        }

        private void ResetFameCounterByMapChangeIfActive()
        {
            if (_mainWindowViewModel.IsTrackingResetByMapChangeActive)
            {
                _mainWindowViewModel.ResetMainCounters();
            }
        }

        private void SetCharacterTrackedVisibility(string name)
        {
            if (string.IsNullOrEmpty(SettingsController.CurrentSettings.MainTrackingCharacterName) || name == SettingsController.CurrentSettings.MainTrackingCharacterName)
            {
                _mainWindowViewModel.CharacterIsNotTrackedInfoVisibility = Visibility.Collapsed;
            }
            else
            {
                _mainWindowViewModel.CharacterIsNotTrackedInfoVisibility = Visibility.Visible;
            }
        }
    }
}