using System;

namespace StatisticsAnalysisTool.Trade.Mails;

[Obsolete]
public class MailOld
{
    public long Tick { get; set; }
    public Guid Guid { get; set; }
    public long MailId { get; set; }
    public string ClusterIndex { get; set; }
    public string MailTypeText { get; set; }
    public MailContent MailContent { get; set; }
}