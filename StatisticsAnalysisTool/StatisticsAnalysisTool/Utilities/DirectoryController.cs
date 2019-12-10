using System;
using System.IO;

namespace StatisticsAnalysisTool.Utilities
{
    class DirectoryController
    {

        public static bool CreateDirectoryWhenNotExists(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
