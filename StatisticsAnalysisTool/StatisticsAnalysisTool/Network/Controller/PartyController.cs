using log4net;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class PartyController
    {
        private readonly TrackingController _trackingController;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //public List<NewCharacterObject> PartyList { get; } = new List<NewCharacterObject>();

        public PartyController(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public void UpdateParty()
        {

        }

        public void ResetParty()
        {

        }
    }
}