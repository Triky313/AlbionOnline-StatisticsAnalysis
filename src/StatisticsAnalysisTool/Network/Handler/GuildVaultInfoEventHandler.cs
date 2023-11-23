using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class GuildVaultInfoEventHandler : EventPacketHandler<GuildVaultInfoEvent>
{
    public GuildVaultInfoEventHandler() : base((int) EventCodes.RecoveryVaultPlayerInfo)
    {
    }

    protected override async Task OnActionAsync(GuildVaultInfoEvent value)
    {
        await Task.CompletedTask;
    }
}