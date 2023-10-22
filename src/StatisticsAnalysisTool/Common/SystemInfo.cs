using Serilog;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace StatisticsAnalysisTool.Common;

public class SystemInfo
{
    public static void LogSystemInfo()
    {
        var osVersion = RuntimeInformation.OSDescription;

        var processor = new ManagementObjectSearcher("select * from Win32_Processor").Get().OfType<ManagementObject>().FirstOrDefault()?["Name"].ToString();

        var videoController = new ManagementObjectSearcher("select * from Win32_VideoController").Get().OfType<ManagementObject>().FirstOrDefault()?["Name"].ToString();

        var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        var isDebug = Debugger.IsAttached;

        var osArchitecture = RuntimeInformation.OSArchitecture;

        var activeNetworks = NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up).Select(nic => nic.Name).ToList();

        Log.Information($"OS Version: {osVersion} - {osArchitecture} | Processor: {processor} | Video: {videoController} | Is Admin: {isAdmin} | Is Debug: {isDebug}");
        Log.Information($"Active Networks: {string.Join(", ", activeNetworks)}");
    }
}