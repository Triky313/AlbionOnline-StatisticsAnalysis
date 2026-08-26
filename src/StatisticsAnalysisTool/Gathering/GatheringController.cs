using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.Gathering;

public class GatheringController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
{
    private const int GatheringRetentionYears = 3;
    private readonly object _sessionSyncRoot = new();
    private GatheringSession _activeSession;
    private int _gatheredCounter;

    public async Task StartSessionAsync(string characterName)
    {
        if (string.IsNullOrWhiteSpace(characterName))
        {
            Log.Warning("Gathering session was not started because the character name is missing");
            return;
        }

        GatheringSession session;
        lock (_sessionSyncRoot)
        {
            if (_activeSession != null
                && string.Equals(_activeSession.CharacterName, characterName, StringComparison.Ordinal))
            {
                return;
            }

            session = new GatheringSession
            {
                Id = Guid.NewGuid(),
                StartedAtUtc = DateTime.UtcNow,
                CharacterName = characterName
            };
            _activeSession = session;
            _activeFishingEvent = null;
        }

        await RefreshSessionFiltersAsync();
        Log.Information("Gathering session started. Character={Character}, SessionId={SessionId}", session.CharacterName, session.Id);
    }

    public async Task EndSessionAsync()
    {
        GatheringSession session;
        lock (_sessionSyncRoot)
        {
            session = _activeSession;
            _activeSession = null;
            _activeFishingEvent = null;
        }

        if (session == null)
        {
            return;
        }

        await RefreshSessionFiltersAsync();
        Log.Information("Gathering session ended. Character={Character}, SessionId={SessionId}", session.CharacterName, session.Id);
    }

    public async Task AddOrUpdateAsync(HarvestFinishedObject harvestFinishedObject)
    {
        if (!SettingsController.CurrentSettings.IsGatheringActive)
        {
            return;
        }

        if (harvestFinishedObject.UserObjectId != trackingController.EntityController.LocalUserData.UserObjectId)
        {
            return;
        }

        var activeSession = GetActiveSession();
        if (activeSession == null)
        {
            Log.Debug("Gathering value discarded because no active session exists");
            return;
        }

        var existingGatheredObject = mainWindowViewModel.GatheringBindings.GatheredCollection
            .FirstOrDefault(x => x.SessionId == activeSession.Id && !x.IsClosed && x.ObjectId == harvestFinishedObject.ObjectId);
        if (existingGatheredObject != null)
        {
            if (existingGatheredObject.EstimatedMarketValue.IntegerValue <= 0)
            {
                var item = ItemController.GetItemByUniqueName(existingGatheredObject.UniqueName);
                existingGatheredObject.EstimatedMarketValue = EstimatedMarketValueController.CalculateNearestToAverage(item.EstimatedMarketValues).MarketValue;
            }
            existingGatheredObject.GainedStandardAmount += harvestFinishedObject.StandardAmount;
            existingGatheredObject.GainedBonusAmount += harvestFinishedObject.CollectorBonusAmount;
            existingGatheredObject.GainedPremiumBonusAmount += harvestFinishedObject.PremiumBonusAmount;
            existingGatheredObject.MiningProcesses++;
        }
        else
        {
            var item = ItemController.GetItemByIndex(harvestFinishedObject.ItemId);
            var gathered = new Gathered()
            {
                SessionId = activeSession.Id,
                CharacterName = activeSession.CharacterName,
                TimestampUtc = DateTime.UtcNow.Ticks,
                UniqueName = item.UniqueName,
                UserObjectId = harvestFinishedObject.UserObjectId,
                ObjectId = harvestFinishedObject.ObjectId,
                EstimatedMarketValue = EstimatedMarketValueController.CalculateNearestToAverage(item.EstimatedMarketValues).MarketValue,
                GainedStandardAmount = harvestFinishedObject.StandardAmount,
                GainedBonusAmount = harvestFinishedObject.CollectorBonusAmount,
                GainedPremiumBonusAmount = harvestFinishedObject.PremiumBonusAmount,
                ClusterIndex = ClusterController.CurrentCluster.Index,
                MapType = ClusterController.CurrentCluster.MapType,
                InstanceName = ClusterController.CurrentCluster.InstanceName,
                MiningProcesses = 1
            };

            await AddGatheredToBindingCollectionAsync(gathered);
            await RemoveExpiredEntriesAsync();
        }

        await SaveInFileAfterExceedingLimit(10);
        mainWindowViewModel.GatheringBindings.UpdateStats();
    }

    public async Task AddGatheredToBindingCollectionAsync(Gathered gathered)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            mainWindowViewModel?.GatheringBindings?.GatheredCollection.Add(gathered);
        });
    }

    public async Task RemoveExpiredEntriesAsync()
    {
        var cutoffUtcTicks = DateTime.UtcNow.AddYears(-GatheringRetentionYears).Ticks;
        var entriesWereRemoved = false;
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var expiredEntries = mainWindowViewModel.GatheringBindings.GatheredCollection
                .Where(x => x.TimestampUtc < cutoffUtcTicks)
                .ToList();
            entriesWereRemoved = expiredEntries.Count > 0;
            mainWindowViewModel.GatheringBindings.GatheredCollection.RemoveRange(expiredEntries);
        });

        if (entriesWereRemoved)
        {
            await RefreshSessionFiltersAsync();
        }
    }

    public async Task ResetSessionAsync()
    {
        Guid sessionToReset;
        lock (_sessionSyncRoot)
        {
            if (_activeSession == null)
            {
                Log.Warning("Gathering session was not reset because no active session exists");
                return;
            }

            sessionToReset = _activeSession.Id;
            var characterName = _activeSession.CharacterName;
            _activeSession = new GatheringSession
            {
                Id = Guid.NewGuid(),
                StartedAtUtc = DateTime.UtcNow,
                CharacterName = characterName
            };
            _activeFishingEvent = null;
        }

        var entriesWereRemoved = false;
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var entriesToRemove = mainWindowViewModel.GatheringBindings.GatheredCollection
                .Where(x => x.SessionId == sessionToReset)
                .ToList();
            entriesWereRemoved = entriesToRemove.Count > 0;
            mainWindowViewModel.GatheringBindings.GatheredCollection.RemoveRange(entriesToRemove);
        });

        await RefreshSessionFiltersAsync();
        if (entriesWereRemoved)
        {
            await SaveInFileAsync();
        }

        mainWindowViewModel.GatheringBindings.UpdateStats();
        Log.Information("Gathering session reset");
    }

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        lock (_sessionSyncRoot)
        {
            if (sessionId == _activeSession?.Id)
            {
                return false;
            }
        }

        var entriesWereRemoved = false;
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var entriesToRemove = mainWindowViewModel.GatheringBindings.GatheredCollection
                .Where(x => x.SessionId == sessionId)
                .ToList();
            entriesWereRemoved = entriesToRemove.Count > 0;
            mainWindowViewModel.GatheringBindings.GatheredCollection.RemoveRange(entriesToRemove);
        });

        if (!entriesWereRemoved)
        {
            return false;
        }

        await SaveInFileAsync();
        await RefreshSessionFiltersAsync();
        mainWindowViewModel.GatheringBindings.UpdateStats();
        Log.Information("Gathering session deleted. SessionId={SessionId}", sessionId);
        return true;
    }

    private GatheringSession GetActiveSession()
    {
        lock (_sessionSyncRoot)
        {
            return _activeSession;
        }
    }

    private async Task RefreshSessionFiltersAsync()
    {
        var activeSession = GetActiveSession();
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            mainWindowViewModel.GatheringBindings.RefreshSessionFilters(
                mainWindowViewModel.GatheringBindings.GatheredCollection.ToList(),
                activeSession);
        });
    }

    public async Task SetGatheredResourcesClosedAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var notClosedGathered = mainWindowViewModel?.GatheringBindings?.GatheredCollection.Where(x => x.IsClosed == false).ToList() ?? new List<Gathered>();
            foreach (Gathered gathered in notClosedGathered)
            {
                gathered.IsClosed = true;
            }
        });
    }

    #region Fishing

    private FishingEvent _activeFishingEvent;

    public void FishingIsStarted(long eventId, int itemIndex)
    {
        var fishingEvent = new FishingEvent
        {
            EventId = eventId,
            UsedFishingRod = itemIndex
        };

        _activeFishingEvent = fishingEvent;
    }

    public void IsCurrentFishingSucceeded(bool isSucceeded)
    {
        if (_activeFishingEvent is { } fishingEvent)
        {
            fishingEvent.IsFishingSucceeded = isSucceeded;
        }
    }

    public void FishingCatchStarted(long actionId)
    {
        if (_activeFishingEvent is { } fishingEvent)
        {
            fishingEvent.HasFishBitten = true;
            fishingEvent.CatchActionId = actionId;
            fishingEvent.DiscoveredFishingItems.Clear();
            fishingEvent.ConfirmedFishingItems.Clear();
        }
    }

    public void AddRewardItem(int itemIndex, int quantity)
    {
        if (_activeFishingEvent is not { HasFishBitten: true } fishingEvent)
        {
            return;
        }

        var itemToAdd = fishingEvent.DiscoveredFishingItems.FirstOrDefault(x => x.ItemIndex == itemIndex);
        if (itemToAdd == null)
        {
            return;
        }

        itemToAdd.Quantity = quantity;
        fishingEvent.ConfirmedFishingItems.Add(itemToAdd);
        fishingEvent.DiscoveredFishingItems.Remove(itemToAdd);
    }

    public void AddFishedItem(DiscoveredItem item)
    {
        if (item == null || _activeFishingEvent is not { HasFishBitten: true } fishingEvent || fishingEvent.UsedFishingRod == item.ItemIndex)
        {
            return;
        }

        fishingEvent.DiscoveredFishingItems.Add(item);
    }

    public async Task FishingFinishedAsync()
    {
        if (_activeFishingEvent is not { IsFishingSucceeded: true } fishingEvent)
        {
            _activeFishingEvent = null;
            return;
        }

        var trackingEventId = fishingEvent.CatchActionId > 0 ? fishingEvent.CatchActionId : fishingEvent.EventId;
        var activeSession = GetActiveSession();
        if (activeSession == null)
        {
            _activeFishingEvent = null;
            Log.Debug("Fishing value discarded because no active gathering session exists");
            return;
        }

        var itemCount = 0;
        foreach (DiscoveredItem confirmedDiscoveredItem in fishingEvent.ConfirmedFishingItems)
        {
            var fishedItem = ItemController.GetItemByIndex(confirmedDiscoveredItem.ItemIndex);
            if (fishedItem == null)
            {
                _activeFishingEvent = null;
                return;
            }

            var gathered = new Gathered()
            {
                SessionId = activeSession.Id,
                CharacterName = activeSession.CharacterName,
                TimestampUtc = fishingEvent.CreateAt.Ticks,
                UniqueName = fishedItem.UniqueName,
                UserObjectId = -1,
                ObjectId = trackingEventId + itemCount,
                EstimatedMarketValue = EstimatedMarketValueController.CalculateNearestToAverage(fishedItem.EstimatedMarketValues).MarketValue,
                GainedStandardAmount = confirmedDiscoveredItem.Quantity,
                GainedBonusAmount = 0,
                GainedPremiumBonusAmount = 0,
                ClusterIndex = ClusterController.CurrentCluster.Index,
                MapType = ClusterController.CurrentCluster.MapType,
                InstanceName = ClusterController.CurrentCluster.InstanceName,
                MiningProcesses = 0,
                HasBeenFished = true
            };

            await AddGatheredToBindingCollectionAsync(gathered);
            itemCount++;
        }

        fishingEvent.DiscoveredFishingItems.Clear();
        _activeFishingEvent = null;

        await RemoveExpiredEntriesAsync();
        await SaveInFileAfterExceedingLimit(10);
        mainWindowViewModel.GatheringBindings.UpdateStats();
    }

    public class FishingEvent
    {
        public DateTime CreateAt { get; init; }
        public long EventId { get; init; }
        public int UsedFishingRod { get; set; }
        public long CatchActionId { get; set; }
        public Item UsedFishingRodItem => ItemController.GetItemByIndex(UsedFishingRod);
        public bool HasFishBitten { get; set; }
        public bool IsFishingSucceeded { get; set; }
        public List<DiscoveredItem> DiscoveredFishingItems { get; } = [];
        public List<DiscoveredItem> ConfirmedFishingItems { get; } = [];

        public FishingEvent()
        {
            CreateAt = DateTime.UtcNow;
        }
    }

    #endregion

    #region Save / Load data

    public async Task LoadFromFileAsync()
    {
        var gatheredDtos = await FileController.LoadAsync<List<GatheredDto>>(AppDataPaths.UserDataFile(Settings.Default.GatheringFileName)) ?? [];
        var cutoffUtcTicks = DateTime.UtcNow.AddYears(-GatheringRetentionYears).Ticks;
        var retainedGatheredDtos = gatheredDtos.Where(x => x.Timestamp >= cutoffUtcTicks).ToList();
        var gathered = retainedGatheredDtos.Select(GatheringMapping.Mapping).ToList();
        await SetGatheredToBindings(gathered);

        if (retainedGatheredDtos.Count != gatheredDtos.Count)
        {
            await SaveInFileAsync();
        }

        await RefreshSessionFiltersAsync();
    }

    public async Task SaveInFileAsync()
    {
        if (!AppDataPaths.TryEnsureUserDataDirectory())
        {
            return;
        }

        var cutoffUtcTicks = DateTime.UtcNow.AddYears(-GatheringRetentionYears).Ticks;
        var gatheredToSave = mainWindowViewModel.GatheringBindings?.GatheredCollection
            .Where(x => x.TimestampUtc >= cutoffUtcTicks)
            .ToList()
            .Select(GatheringMapping.Mapping);

        await FileController.SaveAsync(gatheredToSave,
            AppDataPaths.UserDataFile(Settings.Default.GatheringFileName));
        Log.Information("Gathering saved");
    }

    public async Task SaveInFileAfterExceedingLimit(int limit)
    {
        if (++_gatheredCounter < limit)
        {
            return;
        }

        if (mainWindowViewModel?.GatheringBindings?.GatheredCollection == null)
        {
            return;
        }

        var gatheredCollection = mainWindowViewModel.GatheringBindings.GatheredCollection;
        var gatheredDtos = gatheredCollection?.Select(GatheringMapping.Mapping).ToList();

        if (gatheredDtos == null)
        {
            return;
        }

        if (!AppDataPaths.TryEnsureUserDataDirectory())
        {
            return;
        }

        await FileController.SaveAsync(gatheredDtos, AppDataPaths.UserDataFile(Settings.Default.GatheringFileName));
        _gatheredCounter = 0;
    }

    private async Task SetGatheredToBindings(IEnumerable<Gathered> gathered)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var enumerable = gathered as Gathered[] ?? gathered.ToArray();
            mainWindowViewModel?.GatheringBindings?.GatheredCollection?.Clear();
            mainWindowViewModel?.GatheringBindings?.GatheredCollection?.AddRange(enumerable.AsEnumerable());
        }, DispatcherPriority.Loaded, CancellationToken.None);
    }

    #endregion
}
