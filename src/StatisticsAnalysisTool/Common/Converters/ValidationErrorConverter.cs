using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Common.Converters;

public class ValidationErrorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is System.Collections.ObjectModel.ReadOnlyObservableCollection<ValidationError> errors && errors.Count > 0)
        {
            return errors[0].ErrorContent;
        }

        return null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}