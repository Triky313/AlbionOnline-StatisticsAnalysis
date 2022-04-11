using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class MailInfoObject
{
    public MailInfoObject(long mailId, string clusterIndex, MailType mailType)
    {
        MailId = mailId;
        ClusterIndex = clusterIndex;
        MailType = mailType;
    }

    public long MailId { get; set; }
    public string ClusterIndex { get; set; }
    public MailType MailType { get; set; }
}