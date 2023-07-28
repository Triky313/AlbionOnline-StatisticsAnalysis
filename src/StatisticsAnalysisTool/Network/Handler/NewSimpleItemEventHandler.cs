using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewSimpleItemEventHandler : EventPacketHandler<NewSimpleItemEvent>
{
    private readonly TrackingController _trackingController;

    public NewSimpleItemEventHandler(TrackingController trackingController) : base((int) EventCodes.NewSimpleItem)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(NewSimpleItemEvent value)
    {
        if (_trackingController.IsTrackingAllowedByMainCharacter())
        {
            _trackingController.VaultController.Add(value.Item);
        }

        EstimatedMarketValueController.Add(value.Item.ItemIndex, value.Item.EstimatedMarketValueInternal, value.Item.Quality);
        _trackingController.LootController.AddDiscoveredItem(value.Item);
        _trackingController.DungeonController.AddDiscoveredItem(value.Item);
        _trackingController.GatheringController.AddFishedItem(value.Item);
        await Task.CompletedTask;
    }
}