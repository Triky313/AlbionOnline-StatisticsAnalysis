using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace StatisticsAnalysisTool.Common;

public static class AppInstance
{
    public static string InstanceId
    {
        get
        {
#if DEBUG
            return "Debug";
#else
            var path = AppContext.BaseDirectory.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(path.ToUpperInvariant()));

            return Convert.ToHexString(hash)[..8];
#endif
        }
    }
}