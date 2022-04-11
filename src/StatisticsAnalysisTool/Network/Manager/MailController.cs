using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using StatisticsAnalysisTool.ViewModels;
using System.Reflection;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class MailController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly TrackingController _trackingController;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public List<Mail> Mails = new();
        public List<MailInfoObject> CurrentMailInfos = new();

        public MailController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void SetMailInfos(List<MailInfoObject> currentMailInfos)
        {
            CurrentMailInfos.Clear();
            CurrentMailInfos.AddRange(currentMailInfos);
        }

        public void AddMail(long mailId, string content)
        {
            if (Mails.Any(x => x.MailId == mailId))
            {
                return;
            }
            
            var mailInfo = CurrentMailInfos.FirstOrDefault(x => x.MailId == mailId);

            if (mailInfo?.MailType == null)
            {
                return;
            }

            var mailContent = ContentToObject(mailInfo.MailType, content);
            Mails.Add(new Mail(mailId, mailInfo.ClusterIndex, mailInfo.MailType, mailContent));
        }

        private static MailContent ContentToObject(MailType type, string content)
        {
            switch (type)
            {
                case MailType.MarketplaceBuyOrderFinished:
                case MailType.MarketplaceSellOrderFinished:
                    var contentObject = content.Split("|");

                    if (contentObject.Length < 3)
                    {
                        return new MailContent();
                    }

                    var quantity = contentObject[0].ObjectToInt();
                    var uniqueItemName = contentObject[1];
                    var totalPrice = FixPoint.FromInternalValue(contentObject[2].ObjectToLong() ?? 0);
                    var unitPrice = FixPoint.FromInternalValue(contentObject[3].ObjectToLong() ?? 0);

                    return new MailContent(quantity, uniqueItemName, totalPrice, unitPrice);

                default:
                    return new MailContent();
            }
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