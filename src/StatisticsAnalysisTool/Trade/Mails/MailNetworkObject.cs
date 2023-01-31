using System;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Trade.Mails;

public class MailNetworkObject
{
    public MailNetworkObject(Guid? guid, long mailId, string subject, string mailTypeText, long tick)
    {
        Guid = guid;
        MailId = mailId;
        Subject = subject;
        MailType = MailController.ConvertToMailType(mailTypeText);
        MailTypeText = mailTypeText;
        Tick = tick;
    }

    public Guid? Guid { get; set; }
    public long MailId { get; set; }
    public string Subject { get; set; }
    public MailType MailType { get; set; }
    public string MailTypeText { get; set; }
    public long Tick { get; set; }
}