using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace StatisticsAnalysisTool.Common
{
    public static class ExtensionMethod
    {
        public static void OrderByReference<T>(this ObservableCollection<T> collection, List<T> comparison)
        {
            for (int i = 0; i < comparison.Count; i++)
            {
                if (!comparison.ElementAt(i).Equals(collection.ElementAt(i)))
                    collection.Move(collection.IndexOf(comparison[i]), i);
            }
        }

        public static void InsertInPlace<T>(this ObservableCollection<T> collection, List<T> comparison, T item)
        {
            int index = comparison.IndexOf(item);
            comparison.RemoveAt(index);
            collection.OrderByReference(comparison);
            collection.Insert(index, item);
        }

        public static string ToShortNumber(this long num) => GetShortNumber(num);

        public static string ToShortNumber(this int num) => GetShortNumber(num);

        public static string ToShortNumber(this double num) => GetShortNumber((decimal)num);

        private static string GetShortNumber(this decimal num)
        {
            if (num < -10000000)
            {
                num /= 10000;
                return (num / 100m).ToString("#.00'M'", CultureInfo.CurrentCulture);
            }

            if (num < -1000000)
            {
                num /= 100;
                return (num / 10m).ToString("#.00'K'", CultureInfo.CurrentCulture);
            }

            if (num < -10000)
            {
                num /= 10;
                return (num / 100m).ToString("#.00'K'", CultureInfo.CurrentCulture);
            }

            if (num < 1000)
            {
                return num.ToString("N0", CultureInfo.CurrentCulture);
            }

            if (num < 10000)
            {
                num /= 10;
                return (num / 100m).ToString("#.00'K'", CultureInfo.CurrentCulture);
            }

            if (num < 1000000)
            {
                num /= 100;
                return (num / 10m).ToString("#.00'K'", CultureInfo.CurrentCulture);
            }

            if (num < 10000000)
            {
                num /= 10000;
                return (num / 100m).ToString("#.00'M'", CultureInfo.CurrentCulture);
            }

            num /= 100000;
            return (num / 10m).ToString("#.00'M'", CultureInfo.CurrentCulture);
        }

        public static DateTime? GetHighestDateTime(this ObservableCollection<DateTime> list)
        {
            if (!list.Any())
            {
                return null;
            }

            return list.Max();
        }

        public static Guid? ObjectToGuid(this object value)
        {
            try
            {
                var valueEnumerable = (IEnumerable)value;
                var myBytes = valueEnumerable.OfType<byte>().ToArray();
                return new Guid(myBytes);
            }
            catch
            {
                return null;
            }
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