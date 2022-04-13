using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

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
            if (Mails.ToArray().Any(x => x.MailId == mailId))
            {
                return;
            }

            var mailInfo = CurrentMailInfos.FirstOrDefault(x => x.MailId == mailId);

            if (mailInfo == null)
            {
                return;
            }

            var mailContent = ContentToObject(mailInfo.MailType, content);

            var mail = new Mail(mailInfo.Tick, mailInfo.Guid, mailId, mailInfo.Subject, mailInfo.MailTypeText, mailContent);

            Mails.Add(mail);
            _mainWindowViewModel.AddMail(mail);
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

                    _ = int.TryParse(contentObject[0], out var quantity);
                    var uniqueItemName = contentObject[1];
                    _ = long.TryParse(contentObject[2], out var totalPriceLong);
                    _ = long.TryParse(contentObject[3], out var unitPriceLong);

                    return new MailContent(quantity, uniqueItemName, totalPriceLong, unitPriceLong);

                default:
                    return new MailContent();
            }
        }

        /// <summary>
        /// Converted a string to MailType.
        /// </summary>
        /// <param name="typeString"></param>
        /// <returns>Returns a enum as MailType.</returns>
        public static MailType ConvertToMailType(string typeString)
        {
            return typeString switch
            {
                "MARKETPLACE_BUYORDER_FINISHED_SUMMARY" => MailType.MarketplaceBuyOrderFinished,
                "MARKETPLACE_SELLORDER_FINISHED_SUMMARY" => MailType.MarketplaceSellOrderFinished,
                _ => MailType.Unknown
            };
        }

        #region Load / Save local file data

        public void LoadFromFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.MailsFileName}";

            if (File.Exists(localFilePath))
            {
                try
                {
                    var localFileString = File.ReadAllText(localFilePath, Encoding.UTF8);
                    var stats = JsonSerializer.Deserialize<List<Mail>>(localFileString) ?? new List<Mail>();
                    Mails = stats;
                    return;
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Mails = new List<Mail>();
                    return;
                }
            }

            Mails = new List<Mail>();
        }

        public void SaveInFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.MailsFileName}";

            try
            {
                var fileString = JsonSerializer.Serialize(Mails);
                File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        #endregion
    }
}