using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Handler;

public class JoinResponseHandler : ResponsePacketHandler<JoinResponse>
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly TrackingController _trackingController;

    public JoinResponseHandler(TrackingController trackingController) : base((int) OperationCodes.Join)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
    }

    protected override async Task OnActionAsync(JoinResponse value)
    {
        await SetLocalUserData(value);

        _trackingController.ClusterController.SetJoinClusterInformation(value.MapIndex, value.MainMapIndex);

        _mainWindowViewModel.UserTrackingBindings.Username = value.Username;
        _mainWindowViewModel.UserTrackingBindings.GuildName = value.GuildName;
        _mainWindowViewModel.UserTrackingBindings.AllianceName = value.AllianceName;

        SetCharacterTrackedVisibility(value.Username);

        _mainWindowViewModel.DungeonBindings.DungeonCloseTimer.Visibility = Visibility.Collapsed;

        await AddEntityAsync(value.UserObjectId, value.UserGuid, value.InteractGuid,
            value.Username, value.GuildName, value.AllianceName, GameObjectType.Player, GameObjectSubType.LocalPlayer);

        _trackingController.DungeonController?.AddDungeonAsync(value.MapType, value.MapGuid).ConfigureAwait(false);

        ResetFameCounterByMapChangeIfActive();
        SetTrackingActivityText();

        await _mainWindowViewModel?.PlayerInformationBindings?.LoadLocalPlayerDataAsync(value.Username)!;
    }

    private async Task SetLocalUserData(JoinResponse value)
    {
        await _trackingController.EntityController.LocalUserData.SetValuesAsync(new LocalUserData
        {
            UserObjectId = value.UserObjectId,
            Guid = value.UserGuid,
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
            IsReSpecActive = value.IsReSpecActive
        });
    }

    private async Task AddEntityAsync(long? userObjectId, Guid? guid, Guid? interactGuid, string name, string guild, string alliance, GameObjectType gameObjectType, GameObjectSubType gameObjectSubType)
    {
        if (guid == null || interactGuid == null || userObjectId == null)
        {
            return;
        }

        _trackingController.EntityController.AddEntity((long) userObjectId, (Guid) guid, interactGuid, name, guild, alliance, null, GameObjectType.Player, GameObjectSubType.LocalPlayer);
        await _trackingController.EntityController.AddToPartyAsync((Guid) guid);
    }

    private void SetTrackingActivityText()
    {
        if (_trackingController.ExistIndispensableInfos)
        {
            _mainWindowViewModel.TrackingActivityBindings.TrackingActiveText = MainWindowTranslation.TrackingIsActive;
            _mainWindowViewModel.TrackingActivityBindings.TrackingActivityType = TrackingIconType.On;
        }
    }

    private void ResetFameCounterByMapChangeIfActive()
    {
        if (_mainWindowViewModel.IsTrackingResetByMapChangeActive)
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            trackingController?.LiveStatsTracker?.Reset();
        }
    }

    private void SetCharacterTrackedVisibility(string name)
    {
        if (string.IsNullOrEmpty(SettingsController.CurrentSettings.MainTrackingCharacterName) || name == SettingsController.CurrentSettings.MainTrackingCharacterName)
        {
            _mainWindowViewModel.TrackingActivityBindings.CharacterIsNotTrackedInfoVisibility = Visibility.Collapsed;
        }
        else
        {
            _mainWindowViewModel.TrackingActivityBindings.CharacterIsNotTrackedInfoVisibility = Visibility.Visible;
        }
    }
}