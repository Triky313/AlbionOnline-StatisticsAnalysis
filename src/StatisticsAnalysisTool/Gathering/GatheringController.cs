using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
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
            existingGatheredObject.GainedStandardAmount += harvestFinishedObject.StandardAmount;
            existingGatheredObject.GainedBonusAmount += harvestFinishedObject.CollectorBonusAmount;
            existingGatheredObject.GainedPremiumBonusAmount += harvestFinishedObject.PremiumBonusAmount;
            existingGatheredObject.MiningProcesses++;
        }
        else
        {
            var gathered = new Gathered()
            {
                Timestamp = DateTime.UtcNow.Ticks,
                UniqueName = ItemController.GetItemUniqueNameByIndex(harvestFinishedObject.ItemId),
                UserObjectId = harvestFinishedObject.UserObjectId,
                ObjectId = harvestFinishedObject.ObjectId,
                GainedStandardAmount = harvestFinishedObject.StandardAmount,
                GainedBonusAmount = harvestFinishedObject.CollectorBonusAmount,
                GainedPremiumBonusAmount = harvestFinishedObject.PremiumBonusAmount,
                ClusterIndex = ClusterController.CurrentCluster.Index,
                MiningProcesses = 1
            };

            AddGatheredToBindingCollection(gathered);
        }

        await SaveInFileAfterExceedingLimit(10);
        _mainWindowViewModel.GatheringBindings.UpdateStats();
    }

    public async void AddGatheredToBindingCollection(Gathered gathered)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            _mainWindowViewModel?.GatheringBindings?.GatheredCollection.Add(gathered);
            //_mainWindowViewModel?.GatheringBindings?.GatheredCollectionView?.Refresh();
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
            .Where(x => safeMoreThan356Days && x.TimestampDateTime > DateTime.UtcNow.AddDays(-365) || !safeMoreThan356Days)
            .ToList()
            .Select(GatheringMapping.Mapping);

        await FileController.SaveAsync(gatheredToSave,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.GatheringFileName));
    }

    public async Task SaveInFileAfterExceedingLimit(int limit)
    {
        if (++_gatheredCounter < limit)
        {
            return;
        }

        if (_mainWindowViewModel?.TradeMonitoringBindings?.Trades == null)
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
        }, DispatcherPriority.Background, CancellationToken.None);
    }

    #endregion
}