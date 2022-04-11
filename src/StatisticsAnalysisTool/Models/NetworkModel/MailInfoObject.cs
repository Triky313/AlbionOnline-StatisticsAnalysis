using System;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class MailInfoObject
{
    public MailInfoObject(Guid? guid, long mailId, string clusterIndex, MailType mailType)
    {
        Guid = guid;
        MailId = mailId;
        ClusterIndex = clusterIndex;
        MailType = mailType;
    }

    public Guid? Guid { get; set; }
    public long MailId { get; set; }
    public string ClusterIndex { get; set; }
    public MailType MailType { get; set; }
}