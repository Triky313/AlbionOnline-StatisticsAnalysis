using System;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.Models;

public class Mail
{
    public Mail(long mailId, string clusterIndex, MailType mailType, MailContent mailContent)
    {
        Timestamp = DateTime.UtcNow;
        MailId = mailId;
        ClusterIndex = clusterIndex;
        MailType = mailType;
        MailContent = mailContent;
    }

    public DateTime Timestamp { get; init; }
    public long MailId { get; set; }
    public string ClusterIndex { get; set; }
    public MailType MailType { get; set; }
    public MailContent MailContent { get; set; }
}