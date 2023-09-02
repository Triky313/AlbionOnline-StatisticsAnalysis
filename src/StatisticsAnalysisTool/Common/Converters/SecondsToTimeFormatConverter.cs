using System.Globalization;
using System.Windows.Data;
using System;

namespace StatisticsAnalysisTool.Common.Converters;

public class SecondsToTimeFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int seconds)
        {
            return "00:00:00";
        }

        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return time.ToString("hh\\:mm\\:ss");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}