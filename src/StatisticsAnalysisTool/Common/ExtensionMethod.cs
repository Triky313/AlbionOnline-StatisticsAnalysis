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
using System.Windows;

namespace StatisticsAnalysisTool.Common;

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

    public static Visibility BoolToVisibility(this bool status)
    {
        return (status) ? Visibility.Visible : Visibility.Collapsed;
    }

    public static Visibility BoolToVisibility(this bool? status)
    {
        return (status ?? false).BoolToVisibility();
    }

    public static double GetValuePerHour(this double value, double seconds)
    {
        try
        {
            var hours = seconds / 60d / 60d;
            return value / hours;
        }
        catch (OverflowException)
        {
            return double.MaxValue;
        }
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

    public static string ToShortNumberString(this object num)
    {
        if (num is long l)
        {
            return GetShortNumber(l);
        }

        if (num is int i)
        {
            return GetShortNumber(i);
        }

        if (num is not double d)
        {
            return double.MaxValue.ToString(CultureInfo.InvariantCulture);
        }

        if (double.IsNaN(d))
        {
            return "0";
        }

        return double.IsInfinity(d) ? double.MaxValue.ToString(CultureInfo.InvariantCulture) : GetShortNumber((decimal) d);

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

    public static string GetShortNumber(this decimal num, CultureInfo culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;

        if (num < -10000000)
        {
            num /= 10000;
            return (num / 100m).ToString("#.##'M'", culture);
        }

        if (num < -1000000)
        {
            num /= 100;
            return (num / 10m).ToString("#.##'K'", culture);
        }

        if (num < -10000)
        {
            num /= 10;
            return (num / 100m).ToString("#.##'K'", culture);
        }

        if (num < 1000)
        {
            return num.ToString("N0", culture);
        }

        if (num < 10000)
        {
            num /= 10;
            return (num / 100m).ToString("#.##'K'", culture);
        }

        if (num < 1000000)
        {
            num /= 100;
            return (num / 10m).ToString("#.##'K'", culture);
        }

        if (num < 10000000)
        {
            num /= 10000;
            return (num / 100m).ToString("#.##'M'", culture);
        }

        num /= 100000;
        return (num / 10m).ToString("#.##'M'", culture);
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

    public static long GetCurrentTotalDamage(this List<KeyValuePair<Guid, PlayerGameObject>> playerObjects)
    {
        return playerObjects.Count <= 0 ? 0 : playerObjects.Max(x => x.Value.Damage);
    }

    public static long GetCurrentTotalHeal(this List<KeyValuePair<Guid, PlayerGameObject>> playerObjects)
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
        var timeSpan = endTime - dateTime;
        var minutes = timeSpan.TotalMinutes;
        var hours = timeSpan.TotalHours;
        var days = timeSpan.TotalDays;

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

        var currentDateTime = DateTime.UtcNow;

        if (dateTime.AddHours(8) <= currentDateTime)
        {
            return ValueTimeStatus.ToOldThird;
        }

        if (dateTime.AddHours(4) <= currentDateTime)
        {
            return ValueTimeStatus.ToOldSecond;
        }

        if (dateTime.AddHours(2) <= currentDateTime)
        {
            return ValueTimeStatus.ToOldFirst;
        }

        return ValueTimeStatus.Normal;
    }

    public static PastTime GetPastTimeEnumByDateTime(this DateTime dateTime)
    {
        var currentDateTime = DateTime.UtcNow;

        if (dateTime.Date <= DateTime.MinValue.Date)
        {
            return PastTime.Unknown;
        }

        if (dateTime.AddDays(30) <= currentDateTime)
        {
            return PastTime.VeryVeryOld;
        }

        if (dateTime.AddDays(7) <= currentDateTime)
        {
            return PastTime.VeryOld;
        }

        if (dateTime.AddHours(24) <= currentDateTime)
        {
            return PastTime.Old;
        }

        if (dateTime.AddHours(8) <= currentDateTime)
        {
            return PastTime.BitOld;
        }

        if (dateTime.AddHours(4) <= currentDateTime)
        {
            return PastTime.LittleNew;
        }

        if (dateTime.AddHours(2) <= currentDateTime)
        {
            return PastTime.AlmostNew;
        }

        return PastTime.New;
    }

    public static bool IsDateInWeekOfYear(this DateTime date1, DateTime date2)
    {
        return date1.Year == date2.Year && ISOWeek.GetWeekOfYear(date1) == ISOWeek.GetWeekOfYear(date2);
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

    #region Lists / Arrays

    public static bool IsInBounds<T>(this IEnumerable<T> array, long index)
    {
        return index >= 0 && index < array.Count() - 1;
    }

    #endregion
}