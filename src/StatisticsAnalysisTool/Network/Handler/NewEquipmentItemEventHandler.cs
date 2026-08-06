using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewEquipmentItemEventHandler(TrackingController trackingController) : EventPacketHandler<NewEquipmentItemEvent>((int) EventCodes.NewEquipmentItem)
{
    protected override async Task OnActionAsync(NewEquipmentItemEvent value)
    {
        if (trackingController.IsTrackingAllowedByMainCharacter())
        {
            trackingController.VaultController.AddDiscoveredItem(value.Item);
        }

        trackingController.EntityController.AddEquipmentItem(new EquipmentItemInternal
        {
            ItemIndex = value.Item.ItemIndex,
            SpellDictionary = value.Item.SpellDictionary
        });

        EstimatedMarketValueController.Add(value.Item.ItemIndex, value.Item.EstimatedMarketValueInternal, value.Item.Quality);

        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel?.CraftingBindings?.BlackMarket?.RecordEstimatedDailyPrice(
            value.Item.ItemIndex,
            (int) value.Item.Quality + 1,
            value.EstimatedBlackMarketValueInternal,
            value.Item.UtcDiscoveryTime);

        trackingController.TrackEquipmentItem(value.Item);

        trackingController.LootController.AddDiscoveredItem(value.Item);
        trackingController.DungeonController.AddDiscoveredItem(value.Item);
        trackingController.GatheringController.AddFishedItem(value.Item);

        await Task.CompletedTask;
    }
}