using System.Security.Principal;

namespace StatisticsAnalysisTool.Core;

public class ApplicationCore
{
    public static bool IsAppStartedAsAdministrator()
    {
        var windowsIdentity = new WindowsPrincipal(WindowsIdentity.GetCurrent());
        return windowsIdentity.IsInRole(WindowsBuiltInRole.Administrator);
    }
}