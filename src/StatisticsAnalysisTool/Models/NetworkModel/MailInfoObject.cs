using System;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class MailInfoObject
{
    public MailInfoObject(Guid? guid, long mailId, string subject, string mailTypeText, string tick)
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
    public string Tick { get; set; }
}