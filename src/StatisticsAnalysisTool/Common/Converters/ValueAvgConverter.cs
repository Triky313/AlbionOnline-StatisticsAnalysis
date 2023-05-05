using System;
using System.Globalization;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Common.Converters;

public class ValueAvgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            double.NaN => $"{0:N0} Avg.".ToString(CultureInfo.CurrentCulture),
            double doubleValue when double.IsInfinity(doubleValue) => $"{double.MaxValue:N0} Avg.".ToString(CultureInfo.CurrentCulture),
            _ => $"{value:N0} Avg.".ToString(CultureInfo.CurrentCulture)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}