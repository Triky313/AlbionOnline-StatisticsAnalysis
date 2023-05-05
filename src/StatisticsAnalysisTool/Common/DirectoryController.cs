using System;
using System.IO;

namespace StatisticsAnalysisTool.Common;

internal static class DirectoryController
{
    public static bool CreateDirectoryWhenNotExists(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
        {
            return false;
        }

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string[] GetFiles(string path, string searchPattern)
    {
        try
        {
            return Directory.GetFiles(path, searchPattern);
        }
        catch (Exception)
        {
            return null;
        }
    }
}