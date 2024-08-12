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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.Gathering;

public class GatheringController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private int _gatheredCounter;

    public GatheringController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public async Task AddOrUpdateAsync(HarvestFinishedObject harvestFinishedObject)
    {
        if (!SettingsController.CurrentSettings.IsGatheringActive)
        {
            return;
        }

        if (harvestFinishedObject.UserObjectId != _trackingController.EntityController.LocalUserData.UserObjectId)
        {
            return;
        }

        var existingGatheredObject = _mainWindowViewModel.GatheringBindings.GatheredCollection.FirstOrDefault(x => !x.IsClosed && x.ObjectId == harvestFinishedObject.ObjectId);
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
                Timestamp = DateTime.UtcNow.Ticks,
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
        _mainWindowViewModel.GatheringBindings.UpdateStats();
    }

    public async void AddGatheredToBindingCollection(Gathered gathered)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            _mainWindowViewModel?.GatheringBindings?.GatheredCollection.Add(gathered);
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
                    var entriesToDelete7Days = _mainWindowViewModel?.GatheringBindings?.GatheredCollection.ToList().Where(x => x.Timestamp < DateTime.UtcNow.AddDays(-7).Ticks);
                    _mainWindowViewModel?.GatheringBindings?.GatheredCollection.RemoveRange(entriesToDelete7Days);
                    break;
                case AutoDeleteGatheringStats.DeleteAfter14Days:
                    var entriesToDelete14Days = _mainWindowViewModel?.GatheringBindings?.GatheredCollection.ToList().Where(x => x.Timestamp < DateTime.UtcNow.AddDays(-14).Ticks);
                    _mainWindowViewModel?.GatheringBindings?.GatheredCollection.RemoveRange(entriesToDelete14Days);
                    break;
                case AutoDeleteGatheringStats.DeleteAfter30Days:
                    var entriesToDelete30Days = _mainWindowViewModel?.GatheringBindings?.GatheredCollection.ToList().Where(x => x.Timestamp < DateTime.UtcNow.AddDays(-30).Ticks);
                    _mainWindowViewModel?.GatheringBindings?.GatheredCollection.RemoveRange(entriesToDelete30Days);
                    break;
                case AutoDeleteGatheringStats.DeleteAfter365Days:
                    var entriesToDelete365Days = _mainWindowViewModel?.GatheringBindings?.GatheredCollection.ToList().Where(x => x.Timestamp < DateTime.UtcNow.AddDays(-365).Ticks);
                    _mainWindowViewModel?.GatheringBindings?.GatheredCollection.RemoveRange(entriesToDelete365Days);
                    break;
            }
        });
    }

    public async Task SetGatheredResourcesClosedAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var notClosedGathered = _mainWindowViewModel?.GatheringBindings?.GatheredCollection.Where(x => x.IsClosed == false).ToList() ?? new List<Gathered>();
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

    public void CloseFishingEvent()
    {
        if (_activeFishingEvent is { } fishingEvent)
        {
            fishingEvent.IsClosedForEvents = true;
        }
    }

    public void AddRewardItem(int itemIndex, int quantity)
    {
        if (_activeFishingEvent is not { IsClosedForEvents: false } fishingEvent)
        {
            return;
        }

        var itemToAdd = fishingEvent.DiscoveredFishingItems?.FirstOrDefault(x => x?.ItemIndex == itemIndex);
        if (itemToAdd == null)
        {
            return;
        }

        itemToAdd.Quantity = quantity;
        fishingEvent.ConfirmedFishingItems.Add(itemToAdd);
    }

    public void AddFishedItem(DiscoveredItem item)
    {
        if (_activeFishingEvent is { IsClosedForEvents: true } || _activeFishingEvent?.UsedFishingRod == item?.ObjectId)
        {
            return;
        }

        _activeFishingEvent?.DiscoveredFishingItems?.Add(item);
    }

    public async Task FishingFinishedAsync()
    {
        if (_activeFishingEvent is not { IsFishingSucceeded: true } fishingEvent)
        {
            _activeFishingEvent = null;
            return;
        }

        var itemCount = 0;
        foreach (DiscoveredItem confirmedDiscoveredItem in fishingEvent.ConfirmedFishingItems)
        {
            var fishedItem = ItemController.GetItemByIndex(confirmedDiscoveredItem?.ItemIndex);
            if (fishedItem == null)
            {
                _activeFishingEvent = null;
                return;
            }

            var gathered = new Gathered()
            {
                Timestamp = _activeFishingEvent.CreateAt.Ticks,
                UniqueName = fishedItem.UniqueName,
                UserObjectId = -1,
                ObjectId = fishingEvent.EventId + itemCount,
                EstimatedMarketValue = EstimatedMarketValueController.CalculateNearestToAverage(fishedItem.EstimatedMarketValues).MarketValue,
                GainedStandardAmount = confirmedDiscoveredItem?.Quantity ?? 0,
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

        _activeFishingEvent.DiscoveredFishingItems.Clear();
        _activeFishingEvent = null;

        await RemoveEntriesByAutoDeleteDateAsync();
        await SaveInFileAfterExceedingLimit(10);
        _mainWindowViewModel.GatheringBindings.UpdateStats();
    }

    public class FishingEvent
    {
        public DateTime CreateAt { get; init; }
        public long EventId { get; init; }
        public int UsedFishingRod { get; set; }
        public Item UsedFishingRodItem => ItemController.GetItemByIndex(UsedFishingRod);
        public bool IsFishingSucceeded { get; set; }
        public bool IsClosedForEvents { get; set; }
        public ObservableCollection<DiscoveredItem> DiscoveredFishingItems = new();
        public ObservableCollection<DiscoveredItem> ConfirmedFishingItems = new();

        public FishingEvent()
        {
            CreateAt = DateTime.UtcNow;
        }
    }

    #endregion

    #region Save / Load data

    public async Task LoadFromFileAsync()
    {
        var gatheredDtos = await FileController.LoadAsync<List<GatheredDto>>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.GatheringFileName));
        var gathered = gatheredDtos.Select(GatheringMapping.Mapping).ToList();
        await SetGatheredToBindings(gathered);
    }

    public async Task SaveInFileAsync(bool safeMoreThan356Days = false)
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));

        var gatheredToSave = _mainWindowViewModel.GatheringBindings?.GatheredCollection
            .Where(x => !safeMoreThan356Days && x.TimestampDateTime > DateTime.UtcNow.AddDays(-365) || safeMoreThan356Days)
            .ToList()
            .Select(GatheringMapping.Mapping);

        await FileController.SaveAsync(gatheredToSave,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.GatheringFileName));
        Log.Information("Gathering saved");
    }

    public async Task SaveInFileAfterExceedingLimit(int limit)
    {
        if (++_gatheredCounter < limit)
        {
            return;
        }

        if (_mainWindowViewModel?.GatheringBindings?.GatheredCollection == null)
        {
            return;
        }

        var gatheredCollection = _mainWindowViewModel.GatheringBindings.GatheredCollection;
        var gatheredDtos = gatheredCollection?.Select(GatheringMapping.Mapping).ToList();

        if (gatheredDtos == null)
        {
            return;
        }

        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));
        await FileController.SaveAsync(gatheredDtos, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.GatheringFileName));
        _gatheredCounter = 0;
    }

    private async Task SetGatheredToBindings(IEnumerable<Gathered> gathered)
    {
        await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
        {
            var enumerable = gathered as Gathered[] ?? gathered.ToArray();
            _mainWindowViewModel?.GatheringBindings?.GatheredCollection?.AddRange(enumerable.AsEnumerable());
            _mainWindowViewModel?.GatheringBindings?.GatheredCollectionView?.Refresh();
        }, DispatcherPriority.Loaded, CancellationToken.None);
        _mainWindowViewModel?.GatheringBindings?.GatheredCollectionView?.Refresh();
    }

    #endregion
}