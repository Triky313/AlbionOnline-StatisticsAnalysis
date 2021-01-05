using Albion.Network;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class GuildVaultInfoHandler : EventPacketHandler<GuildVaultInfoEvent>
    {
        public GuildVaultInfoHandler() : base(375) { }

        protected override async Task OnActionAsync(GuildVaultInfoEvent value)
        {
            Debug.Print($"GuildVaultInfo");
        }
    }
}