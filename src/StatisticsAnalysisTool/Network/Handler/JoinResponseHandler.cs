using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.BindingModel;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Handler;

public class JoinResponseHandler(TrackingController trackingController) : ResponsePacketHandler<JoinResponse>((int) OperationCodes.Join)
{
    private readonly MainWindowViewModel _mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();

    protected override async Task OnActionAsync(JoinResponse value)
    {
        trackingController.CancelLogoutDetection();
        var hadActiveStatisticsSession = trackingController.StatisticController.HasActiveSession;

        SetLocalUserData(value);
        trackingController.StatisticController.StartSession(value.Username);
        _ = SetApiUserData(value);

        _mainWindowViewModel.MainStatusBindings.SetInGame(true);
        _mainWindowViewModel.MainStatusBindings.SetCharacter(value.Username);

        var sourceClusterIndex = value.SourceClusterIndex;
        if (value.MapType == Cluster.MapType.StaticDungeon && string.IsNullOrWhiteSpace(sourceClusterIndex))
        {
            sourceClusterIndex = Cluster.ClusterController.CurrentCluster.Index;
        }
        trackingController.ClusterController.SetJoinClusterInformation(value.MapIndex, sourceClusterIndex, value.MapGuid, value.MapType);

        _mainWindowViewModel.UserTrackingBindings.Username = value.Username;
        _mainWindowViewModel.UserTrackingBindings.GuildName = value.GuildName;
        _mainWindowViewModel.UserTrackingBindings.AllianceName = value.AllianceName;

        SetCharacterTrackedVisibility(value.Username);

        await AddEntityAsync(new Entity
        {
            ObjectId = value.UserObjectId,
            UserGuid = value.UserGuid ?? Guid.Empty,
            InteractGuid = value.InteractGuid,
            Name = value.Username,
            Guild = value.GuildName,
            Alliance = value.AllianceName,
            ObjectType = GameObjectType.Player,
            ObjectSubType = GameObjectSubType.LocalPlayer
        });

        await trackingController.DungeonController.AddDungeonAsync(value.MapType, value.MapGuid, sourceClusterIndex, value.SourceExitPosition);

        await ResetSessionByMapChangeIfActiveAsync(hadActiveStatisticsSession);
        SetTrackingActivityText();

        await _mainWindowViewModel?.PlayerInformationBindings?.LoadLocalPlayerDataAsync(value.Username)!;
    }

    private void SetLocalUserData(JoinResponse value)
    {
        trackingController.EntityController.LocalUserData.SetValues(new LocalUserData
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
            MainMapIndex = value.SourceClusterIndex,
            PlayTimeInSeconds = value.PlayTimeInSeconds,
            AllianceName = value.AllianceName,
            IsReSpecActive = value.IsReSpecActive
        });

        _mainWindowViewModel.DamageMeterBindings.SetLocalPlayer(value.UserGuid, value.Username);
    }

    private async Task SetApiUserData(JoinResponse value)
    {
        _mainWindowViewModel.DashboardBindings.KillsDeathsText = DashboardBindings.TranslationKillsDeathsLoading;
        await trackingController.EntityController.LocalUserData.GetApiData(value.Username);
        _mainWindowViewModel.DashboardBindings.KillsDeathsText = DashboardBindings.TranslationKillsDeaths;
        trackingController.StatisticController.SetKillsDeathsValues();
    }

    private async Task AddEntityAsync(Entity entity)
    {
        if (entity?.UserGuid == null || entity.InteractGuid == null || entity.ObjectId == null)
        {
            return;
        }

        trackingController.EntityController.AddEntity(entity);
        await trackingController.EntityController.AddToPartyAsync(entity.UserGuid);
    }

    private void SetTrackingActivityText()
    {
        if (trackingController.ExistIndispensableInfos)
        {
            _mainWindowViewModel.TrackingActivityBindings.TrackingActiveText = MainWindowTranslation.TrackingIsActive;
            _mainWindowViewModel.TrackingActivityBindings.TrackingActivityType = TrackingIconType.On;
        }
    }

    private async Task ResetSessionByMapChangeIfActiveAsync(bool hadActiveStatisticsSession)
    {
        if (_mainWindowViewModel.IsTrackingResetByMapChangeActive && hadActiveStatisticsSession)
        {
            await trackingController.StatisticController.ResetSessionAsync();
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
