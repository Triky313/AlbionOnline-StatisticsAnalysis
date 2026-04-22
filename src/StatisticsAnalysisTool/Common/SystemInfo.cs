using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace StatisticsAnalysisTool.Common;

public class SystemInfo
{
    public static void LogSystemInfo()
    {
        try
        {
            var osVersion = RuntimeInformation.OSDescription;
            var osArchitecture = RuntimeInformation.OSArchitecture;
            var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            var isDebug = Debugger.IsAttached;
            var processor = GetProcessorName();
            var videoController = GetVideoControllerName();
            var activeNetworks = GetActiveNetworkNames();

            Log.Information("OS Version: {OSVersion} - {OSArchitecture} | Processor: {Processor} | Video: {VideoController} | Is Admin: {IsAdmin} | Is Debug: {IsDebug} | Active Networks: {ActiveNetworks}",
                osVersion, osArchitecture, processor, videoController, isAdmin, isDebug, activeNetworks);
        }
        catch (Exception ex)
        {
            Log.Error("An unexpected error occurred in LogSystemInfo: {Error}", ex.Message);
        }
    }

    public static string CreateReport()
    {
        var textBuilder = new StringBuilder();

        textBuilder.AppendLine("System data:");
        textBuilder.AppendLine($"OS version: {RuntimeInformation.OSDescription}");
        textBuilder.AppendLine($"OS architecture: {RuntimeInformation.OSArchitecture}");
        textBuilder.AppendLine($"Process architecture: {RuntimeInformation.ProcessArchitecture}");
        textBuilder.AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}");
        textBuilder.AppendLine($"Processor count: {Environment.ProcessorCount}");
        textBuilder.AppendLine($"Processor: {GetProcessorName()}");
        textBuilder.AppendLine($"Video: {GetVideoControllerName()}");
        textBuilder.AppendLine($"Is admin: {new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)}");
        textBuilder.AppendLine($"Is debug: {Debugger.IsAttached}");
        textBuilder.AppendLine($"Active networks: {GetActiveNetworkNames()}");

        return textBuilder.ToString();
    }

    private static string GetProcessorName()
    {
        try
        {
            using var processorSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");

            return processorSearcher
                .Get()
                .OfType<ManagementObject>()
                .FirstOrDefault()?["Name"]?.ToString() ?? "Unknown";
        }
        catch (Exception ex)
        {
            Log.Warning("Failed to retrieve processor info: {Error}", ex.Message);
            return "Unknown";
        }
    }

    private static string GetVideoControllerName()
    {
        try
        {
            using var videoControllerSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");

            return videoControllerSearcher
                .Get()
                .OfType<ManagementObject>()
                .FirstOrDefault()?["Name"]?.ToString() ?? "Unknown";
        }
        catch (Exception ex)
        {
            Log.Warning("Failed to retrieve video controller info: {Error}", ex.Message);
            return "Unknown";
        }
    }

    private static string GetActiveNetworkNames()
    {
        return string.Join(", ", NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
            .Select(nic => nic.Name));
    }
}