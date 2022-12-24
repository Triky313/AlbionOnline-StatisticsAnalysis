using System;
using System.Globalization;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Common.Converters;

public class ShortNumberConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!decimal.TryParse(value?.ToString(), out var doubleNumber))
        {
            throw new ArgumentNullException();
        }
        
        return doubleNumber.GetShortNumber(culture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}