using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Common
{
    public static class ExtensionMethod
    {
        public static long ObjectToLong(this object value)
        {
            return value as short? ?? value as int? ?? value as long? ?? 0;
        }

        public static int ObjectToInt(this object value)
        {
            return value as short? ?? value as int? ?? 0;
        }

        public static short ObjectToShort(this object value)
        {
            return value as short? ?? 0;
        }

        public static double ObjectToDouble(this object value)
        {
            return value as float? ?? value as double? ?? 0;
        }

        public static Dictionary<int, T> ToDictionary<T>(this IEnumerable<T> array)
        {
            return array
                .Select((v, i) => new { Key = i, Value = v })
                .ToDictionary(o => o.Key, o => o.Value);
        }
    }
}