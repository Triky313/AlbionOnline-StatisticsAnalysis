using Serilog;
using System;
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
        try
        {
            var osVersion = RuntimeInformation.OSDescription;
            var osArchitecture = RuntimeInformation.OSArchitecture;
            var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            var isDebug = Debugger.IsAttached;

            string processor = "Unknown";
            try
            {
                processor = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor")
                    .Get()
                    .OfType<ManagementObject>()
                    .FirstOrDefault()?["Name"]?.ToString() ?? "Unknown";
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to retrieve processor info: {Error}", ex.Message);
            }

            string videoController = "Unknown";
            try
            {
                videoController = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController")
                    .Get()
                    .OfType<ManagementObject>()
                    .FirstOrDefault()?["Name"]?.ToString() ?? "Unknown";
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to retrieve video controller info: {Error}", ex.Message);
            }

            var activeNetworks = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
                .Select(nic => nic.Name)
                .ToList();

            Log.Information("OS Version: {OSVersion} - {OSArchitecture} | Processor: {Processor} | Video: {VideoController} | Is Admin: {IsAdmin} | Is Debug: {IsDebug} | Active Networks: {ActiveNetworks}",
                osVersion, osArchitecture, processor, videoController, isAdmin, isDebug, string.Join(", ", activeNetworks));
        }
        catch (Exception ex)
        {
            Log.Error("An unexpected error occurred in LogSystemInfo: {Error}", ex.Message);
        }
    }
}