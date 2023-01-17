using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class PartySilverGainedEventHandler
{
    private readonly TrackingController _trackingController;

    public PartySilverGainedEventHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(PartySilverGainedEvent value)
    {
        //Debug.Print($"Total Collected Silver: {value.TotalCollectedSilver}");
        //Debug.Print($"Guild Tax: {value.GuildTax}");
        //Debug.Print($"Earned Silver: {value.EarnedSilver}");

        await Task.CompletedTask;
    }
}