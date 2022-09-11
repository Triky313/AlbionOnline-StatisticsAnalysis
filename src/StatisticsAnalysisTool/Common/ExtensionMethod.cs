using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common
{
    public static class ExtensionMethod
    {
        public static string SetYesOrNo(this bool value)
        {
            return (value) ? LanguageController.Translation("YES") : LanguageController.Translation("NO");
        }

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

        public static Dictionary<int, T> ToDictionary<T>(this IEnumerable<T> array)
        {
            return array
                .Select((v, i) => new { Key = i, Value = v })
                .ToDictionary(o => o.Key, o => o.Value);
        }

        public static string ToTimerString(this TimeSpan span)
        {
            return $"{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";
        }

        public static string ToTimerString(this int seconds)
        {
            var span = new TimeSpan(0, 0, 0, seconds);
            return $"{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";
        }

        #region Object to

        public static Guid? ObjectToGuid(this object value)
        {
            try
            {
                if (value is IEnumerable valueEnumerable)
                {
                    var myBytes = valueEnumerable.OfType<byte>().ToArray();
                    return new Guid(myBytes);
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public static ulong? ObjectToUlong(this object value)
        {
            return value as byte? ?? value as ushort? ?? value as uint? ?? value as ulong?;
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

        #endregion

        #region Number manipulation

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

                return double.IsInfinity(num) ? double.MaxValue.ToString(CultureInfo.InvariantCulture) : GetShortNumber((decimal)num);
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

        public static double ToPositive(this double value)
        {
            return value > 0 ? value : -value;
        }

        public static double ToPositiveFromNegativeOrZero(this double healthChange)
        {
            return healthChange >= 0d ? 0d : healthChange.ToPositive();
        }

        #endregion

        #region Player Objects

        public static long GetHighestDamage(this List<KeyValuePair<Guid, PlayerGameObject>> playerObjects)
        {
            return playerObjects.Count <= 0 ? 0 : playerObjects.Max(x => x.Value.Damage);
        }

        public static long GetHighestHeal(this List<KeyValuePair<Guid, PlayerGameObject>> playerObjects)
        {
            return playerObjects.Count <= 0 ? 0 : playerObjects.Max(x => x.Value.Heal);
        }

        public static double GetDamagePercentage(this List<KeyValuePair<Guid, PlayerGameObject>> playerObjects, double playerDamage)
        {
            var totalDamage = playerObjects.Sum(x => x.Value.Damage);
            return 100.00 / totalDamage * playerDamage;
        }

        public static double GetHealPercentage(this List<KeyValuePair<Guid, PlayerGameObject>> playerObjects, double playerHeal)
        {
            var totalHeal = playerObjects.Sum(x => x.Value.Heal);
            return 100.00 / totalHeal * playerHeal;
        }

        #endregion

        #region DateTime

        public static string CurrentDateTimeFormat(this DateTime value)
        {
            return DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime()
                .ToString("G", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));
        }

        public static string DateTimeToLastUpdateTime(this DateTime dateTime)
        {
            var endTime = DateTime.UtcNow;
            var minutes = (endTime - dateTime).TotalMinutes;
            var hours = (endTime - dateTime).TotalHours;
            var days = (endTime - dateTime).TotalDays;

            if (minutes <= 120) return $"{minutes:N0} {LanguageController.Translation("MINUTES")}";

            if (hours <= 48) return $"{hours:N0} {LanguageController.Translation("HOURS")}";

            if (days <= 365) return $"{days:N0} {LanguageController.Translation("DAYS")}";

            return $"{LanguageController.Translation("OVER_A_YEAR")}";
        }

        public static ValueTimeStatus GetValueTimeStatus(this DateTime dateTime)
        {
            if (dateTime.Date <= DateTime.MinValue.Date)
            {
                return ValueTimeStatus.NoValue;
            }

            if (dateTime.AddHours(8) <= DateTime.UtcNow)
            {
                return ValueTimeStatus.ToOldThird;
            }

            if (dateTime.AddHours(4) <= DateTime.UtcNow)
            {
                return ValueTimeStatus.ToOldSecond;
            }

            if (dateTime.AddHours(2) <= DateTime.UtcNow)
            {
                return ValueTimeStatus.ToOldFirst;
            }

            return ValueTimeStatus.Normal;
        }

        public static bool IsDateInWeekOfYear(this DateTime date1, DateTime date2)
        {
            return ISOWeek.GetWeekOfYear(date1) == ISOWeek.GetWeekOfYear(date2);
        }

        #endregion

        #region Json

        public static async Task<string> SerializeJsonStringAsync(this object obj, JsonSerializerOptions option = null)
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, obj, obj.GetType(), option);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        #endregion

        #region Collections

        public static async Task AddRangeAsync<T>(this ObservableCollection<T> collection, IEnumerable<T> list)
        {
            await foreach (var item in list?.ToAsyncEnumerable() ?? new List<T>().ToAsyncEnumerable())
            {
                collection.Add(item);
            }
        }

        #endregion
    }
}