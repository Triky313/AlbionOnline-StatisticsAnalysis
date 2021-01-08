using Albion.Network;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UserInformationHandler : ResponsePacketHandler<UserInformationEvent>
    {
        public UserInformationHandler() : base(2) { }

        protected override async Task OnActionAsync(UserInformationEvent value)
        {

            await Task.CompletedTask;
        }
    }
}