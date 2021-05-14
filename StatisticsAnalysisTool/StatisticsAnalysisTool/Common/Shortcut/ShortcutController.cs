using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace StatisticsAnalysisTool.Common.Shortcut
{
    public static class ShortcutController
    {
        public static void CreateShortcut()
        {
            var link = (IShellLink) new ShellLink();
            link.SetDescription("Statistics Analysis Tool");
            link.SetPath(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "StatisticsAnalysisTool.exe"));

            var file = (IPersistFile)link;
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            file.Save(Path.Combine(desktopPath, "Statistics Analysis Tool.lnk"), false);
        }
    }
}