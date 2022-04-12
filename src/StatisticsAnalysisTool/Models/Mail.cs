using System;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;

namespace StatisticsAnalysisTool.Models;

public class Mail
{
    public Mail(string tick, Guid? guid, long mailId, string clusterIndex, string mailTypeText, MailContent mailContent)
    {
        Tick = tick;
        Guid = guid ?? default;
        MailId = mailId;
        ClusterIndex = clusterIndex;
        MailTypeText = mailTypeText;
        MailContent = mailContent;
    }

    public string Tick { get; init; }
    [JsonIgnore]
    public DateTime Timestamp => long.TryParse(Tick, out var timeStamp) ? new DateTime(timeStamp) : new DateTime(0);
    public Guid Guid { get; init; }
    public long MailId { get; init; }
    public string ClusterIndex { get; init; }
    public string MailTypeText { get; set; }
    [JsonIgnore]
    public MailType MailType => MailController.ConvertToMailType(MailTypeText);
    public MailContent MailContent { get; set; }
}