﻿using System;
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
            for (var i = 0; i < comparison.Count; i++)
            {
                if (!comparison.ElementAt(i).Equals(collection.ElementAt(i)))
                {
                    collection.Move(collection.IndexOf(comparison[i]), i);
                }
            }
        }
        
        public static string ToShortNumberString(this long num)
        {
            return GetShortNumber(num);
        }

        public static string ToShortNumberString(this int num)
        {
            return GetShortNumber(num);
        }

        public static string ToShortNumberString(this double num)
        {
            try
            {
                if (double.IsNaN(num))
                {
                    return "0";
                }

                return double.IsInfinity(num) ? double.MaxValue.ToString(CultureInfo.InvariantCulture) : GetShortNumber((decimal) num);
            }
            catch (OverflowException)
            {
                return double.MaxValue.ToString(CultureInfo.InvariantCulture);
            }
        }

        public static double ToShortNumber(this double num, double maxNumber = double.MaxValue)
        {
            try
            {
                if (double.IsNaN(num))
                {
                    return maxNumber;
                }

                return double.IsInfinity(num) ? maxNumber : num;
            }
            catch (OverflowException)
            {
                return maxNumber;
            }
        }

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

            if (num < 1000) return num.ToString("N0", CultureInfo.CurrentCulture);

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
                var valueEnumerable = (IEnumerable) value;
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

        public static byte ObjectToByte(this object value)
        {
            return value as byte? ?? 0;
        }

        public static bool ObjectToBool(this object value)
        {
            return value as bool? ?? false;
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
                .Select((v, i) => new {Key = i, Value = v})
                .ToDictionary(o => o.Key, o => o.Value);
        }

        public static string ToTimerString(this TimeSpan span)
        {
            return $"{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";
        }
    }
}