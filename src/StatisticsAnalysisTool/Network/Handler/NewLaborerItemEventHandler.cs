using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewLaborerItemEventHandler : EventPacketHandler<NewLaborerItemEvent>
{
    private readonly TrackingController _trackingController;

    public NewLaborerItemEventHandler(TrackingController trackingController) : base((int) EventCodes.NewLaborerItem)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(NewLaborerItemEvent value)
    {
        if (_trackingController.IsTrackingAllowedByMainCharacter())
        {
            _trackingController.VaultController.AddDiscoveredItem(value.Item);
        }
        
        EstimatedMarketValueController.Add(value.Item.ItemIndex, value.Item.EstimatedMarketValueInternal, value.Item.Quality);
        _trackingController.LootController.AddDiscoveredItem(value.Item);
        _trackingController.DungeonController.AddDiscoveredItem(value.Item);
        await Task.CompletedTask;
    }
}