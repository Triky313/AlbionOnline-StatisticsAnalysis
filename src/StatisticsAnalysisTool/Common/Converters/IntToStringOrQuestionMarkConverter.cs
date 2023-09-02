using System;
using System.Globalization;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Common.Converters;

public class IntToStringOrQuestionMarkConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return "?";
        }

        int intValue = (int) value;
        return intValue == -1 ? "?" : intValue.ToString();
        
        
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}