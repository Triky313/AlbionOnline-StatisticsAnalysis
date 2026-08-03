using System;
using System.Globalization;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Common.Converters;

public sealed class DurationSecondsToCompactStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double seconds;
        try
        {
            seconds = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
        {
            seconds = 0;
        }

        seconds = double.IsFinite(seconds) ? seconds : 0;
        var totalMinutes = Math.Max(0, (long) Math.Floor(seconds / 60));
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;

        return hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}