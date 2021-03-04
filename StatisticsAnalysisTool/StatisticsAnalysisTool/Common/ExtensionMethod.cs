using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.Common
{
    public static class ExtensionMethod
    {
        public static DateTime? GetHighestDateTime(this ObservableCollection<DateTime> list)
        {
            if (!list.Any())
            {
                return null;
            }

            return list.Max();
        }

        public static long? ObjectToLong(this object value)
        {
            return value as byte? ?? value as short? ?? value as int? ?? value as long?;
        }

        public static int ObjectToInt(this object value)
        {
            return value as byte? ?? value as short? ?? value as int? ?? 0;
        }

        public static short ObjectToShort(this object value)
        {
            return value as byte? ?? value as short? ?? 0;
        }

        public static double ObjectToDouble(this object value)
        {
            return value as float? ?? value as double? ?? 0;
        }

        public static double ToPositive(this double value)
        {
            return value > 0 ? value : -value;
        }

        public static double ToPositiveFromNegativeOrZero(this double healthChange)
        {
            return healthChange >= 0d ? 0d : healthChange.ToPositive();
        }

        public static Dictionary<int, T> ToDictionary<T>(this IEnumerable<T> array)
        {
            return array
                .Select((v, i) => new { Key = i, Value = v })
                .ToDictionary(o => o.Key, o => o.Value);
        }
    }
}