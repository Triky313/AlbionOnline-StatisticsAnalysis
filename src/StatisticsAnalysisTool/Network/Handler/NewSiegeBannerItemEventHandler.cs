using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewSiegeBannerItemEventHandler : EventPacketHandler<NewSiegeBannerItemEvent>
{
    public NewSiegeBannerItemEventHandler() : base((int) EventCodes.NewSiegeBannerItem)
    {
    }

    protected override Task OnActionAsync(NewSiegeBannerItemEvent value)
    {
        if (value.Item != null)
        {
            EstimatedMarketValueController.Add(
                value.Item.ItemIndex,
                value.Item.EstimatedMarketValueInternal,
                value.Item.Quality);
        }

        return Task.CompletedTask;
    }
}