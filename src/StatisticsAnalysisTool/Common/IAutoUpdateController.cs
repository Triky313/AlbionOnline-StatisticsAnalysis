using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

public interface IAutoUpdateController
{
    Task AutoUpdateAsync(bool reportErrors = false);
    void RemoveUpdateFiles();
}