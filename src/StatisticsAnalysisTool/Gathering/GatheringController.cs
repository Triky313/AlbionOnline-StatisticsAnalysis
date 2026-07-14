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
    private int _gatheredCounter;

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

        var existingGatheredObject = mainWindowViewModel.GatheringBindings.GatheredCollection.FirstOrDefault(x => !x.IsClosed && x.ObjectId == harvestFinishedObject.ObjectId);
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

            AddGatheredToBindingCollection(gathered);
            await RemoveEntriesByAutoDeleteDateAsync();
        }

        await SaveInFileAfterExceedingLimit(10);
        mainWindowViewModel.GatheringBindings.UpdateStats();
    }

    public async void AddGatheredToBindingCollection(Gathered gathered)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            mainWindowViewModel?.GatheringBindings?.GatheredCollection.Add(gathered);
        });
    }

    public async Task RemoveEntriesByAutoDeleteDateAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            switch (SettingsController.CurrentSettings.AutoDeleteGatheringStats)
            {
                case AutoDeleteGatheringStats.NeverDelete:
                    return;
                case AutoDeleteGatheringStats.DeleteAfter7Days:
                    var entriesToDelete7Days = mainWindowViewModel?.GatheringBindings?.GatheredCollection.ToList().Where(x => x.TimestampUtc < DateTime.UtcNow.AddDays(-7).Ticks);
                    mainWindowViewModel?.GatheringBindings?.GatheredCollection.RemoveRange(entriesToDelete7Days);
                    break;
                case AutoDeleteGatheringStats.DeleteAfter14Days:
                    var entriesToDelete14Days = mainWindowViewModel?.GatheringBindings?.GatheredCollection.ToList().Where(x => x.TimestampUtc < DateTime.UtcNow.AddDays(-14).Ticks);
                    mainWindowViewModel?.GatheringBindings?.GatheredCollection.RemoveRange(entriesToDelete14Days);
                    break;
                case AutoDeleteGatheringStats.DeleteAfter30Days:
                    var entriesToDelete30Days = mainWindowViewModel?.GatheringBindings?.GatheredCollection.ToList().Where(x => x.TimestampUtc < DateTime.UtcNow.AddDays(-30).Ticks);
                    mainWindowViewModel?.GatheringBindings?.GatheredCollection.RemoveRange(entriesToDelete30Days);
                    break;
                case AutoDeleteGatheringStats.DeleteAfter365Days:
                    var entriesToDelete365Days = mainWindowViewModel?.GatheringBindings?.GatheredCollection.ToList().Where(x => x.TimestampUtc < DateTime.UtcNow.AddDays(-365).Ticks);
                    mainWindowViewModel?.GatheringBindings?.GatheredCollection.RemoveRange(entriesToDelete365Days);
                    break;
            }
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

            AddGatheredToBindingCollection(gathered);
            itemCount++;
        }

        fishingEvent.DiscoveredFishingItems.Clear();
        _activeFishingEvent = null;

        await RemoveEntriesByAutoDeleteDateAsync();
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
        var gatheredDtos = await FileController.LoadAsync<List<GatheredDto>>(AppDataPaths.UserDataFile(Settings.Default.GatheringFileName));
        var gathered = gatheredDtos.Select(GatheringMapping.Mapping).ToList();
        await SetGatheredToBindings(gathered);
    }

    public async Task SaveInFileAsync(bool safeMoreThan356Days = false)
    {
        if (!AppDataPaths.TryEnsureUserDataDirectory())
        {
            return;
        }

        var gatheredToSave = mainWindowViewModel.GatheringBindings?.GatheredCollection
            .Where(x => !safeMoreThan356Days && x.TimestampDateTimeUtc > DateTime.UtcNow.AddDays(-365) || safeMoreThan356Days)
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
            mainWindowViewModel?.GatheringBindings?.GatheredCollectionView?.Refresh();
        }, DispatcherPriority.Loaded, CancellationToken.None);
        mainWindowViewModel?.GatheringBindings?.GatheredCollectionView?.Refresh();
    }

    #endregion
}
