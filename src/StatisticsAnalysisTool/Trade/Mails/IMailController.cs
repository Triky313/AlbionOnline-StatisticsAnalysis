using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Trade.Mails;

public interface IMailController
{
    void SetMailInfos(IEnumerable<MailNetworkObject> currentMailInfos);
    Task AddMailAsync(long mailId, string content);
}