using System;
using System.Globalization;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Common.Converters;

public class RoundingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        switch (value)
        {
            case double doubleNumber:
                {
                    int decimalPlaces = parameter != null ? System.Convert.ToInt32(parameter) : 0;
                    double roundedNumber = Math.Round(doubleNumber, decimalPlaces);
                    return roundedNumber;
                }
            case float floatNumber:
                {
                    int decimalPlaces = parameter != null ? System.Convert.ToInt32(parameter) : 0;
                    double roundedNumber = Math.Round(floatNumber, decimalPlaces);
                    return roundedNumber;
                }
            default:
                return value;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}