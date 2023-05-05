using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace StatisticsAnalysisTool.Common.Shortcut;

public static class ShortcutController
{
    public static void CreateShortcut()
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        var link = (IShellLink)new ShellLink();
        link.SetDescription("Statistics Analysis Tool");
        link.SetPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StatisticsAnalysisTool.exe"));

        // ReSharper disable once SuspiciousTypeConversion.Global
        var file = (IPersistFile)link;
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        file.Save(Path.Combine(desktopPath, "Statistics Analysis Tool.lnk"), false);
    }
}