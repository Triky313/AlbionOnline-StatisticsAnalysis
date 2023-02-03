using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Gathering;

public class GatheringController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;

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
        }
        else
        {
            var gathered = new Gathered()
            {
                Timestamp = DateTime.UtcNow.Ticks,
                ItemId = harvestFinishedObject.ItemId,
                ObjectId = harvestFinishedObject.ObjectId,
                GainedStandardAmount = harvestFinishedObject.StandardAmount,
                GainedBonusAmount = harvestFinishedObject.CollectorBonusAmount,
                GainedPremiumBonusAmount = harvestFinishedObject.PremiumBonusAmount,
                ClusterIndex = ClusterController.CurrentCluster.Index
            };

            AddGatheredToBindingCollection(gathered);
        }

        //await _trackingController.TradeController.SaveInFileAfterExceedingLimit(10);
    }

    public async void AddGatheredToBindingCollection(Gathered gathered)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            _mainWindowViewModel?.GatheringBindings?.GatheredCollection.Add(gathered);
            _mainWindowViewModel?.GatheringBindings?.GatheredCollectionView?.Refresh();
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
}