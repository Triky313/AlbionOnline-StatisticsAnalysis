using System;
using System.IO;

namespace StatisticsAnalysisTool.Common
{
    static class DirectoryController
    {

        public static bool CreateDirectoryWhenNotExists(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
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
}
