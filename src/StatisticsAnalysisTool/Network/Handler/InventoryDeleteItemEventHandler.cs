using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class InventoryDeleteItemEventHandler : EventPacketHandler<InventoryDeleteItemEvent>
{
    public InventoryDeleteItemEventHandler() : base((int) EventCodes.InventoryDeleteItem)
    {
    }

    protected override async Task OnActionAsync(InventoryDeleteItemEvent value)
    {
        await Task.CompletedTask;
    }
}