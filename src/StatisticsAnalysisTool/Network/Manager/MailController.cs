using log4net;
using StatisticsAnalysisTool.ViewModels;
using System.Reflection;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class MailController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly TrackingController _trackingController;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public MailController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void AddMailIndexList()
        {

        }

        public void AddMail(string typeString, string content)
        {

        }

        public void ContentToObject(MailType type, string content)
        {

        }

        public static MailType ConvertToMailType(string typeString)
        {
            return typeString switch
            {
                "MARKETPLACE_BUYORDER_FINISHED_SUMMARY" => MailType.MarketplaceBuyOrderFinished,
                "MARKETPLACE_SELLORDER_FINISHED_SUMMARY" => MailType.MarketplaceBuyOrderFinished,
                _ => MailType.Unknown
            };
        }
    }
}