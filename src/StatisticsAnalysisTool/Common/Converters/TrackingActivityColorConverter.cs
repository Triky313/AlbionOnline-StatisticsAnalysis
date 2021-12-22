using StatisticsAnalysisTool.Enumerations;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace StatisticsAnalysisTool.Common.Converters
{
    public class TrackingActivityColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new SolidColorBrush((Color)Application.Current.Resources["Color.Text.6"]);
            }

            if ((TrackingIconType)value == TrackingIconType.On)
            {
                return new SolidColorBrush((Color)Application.Current.Resources["Color.Accent.Green.4"]);
            }

            if ((TrackingIconType)value == TrackingIconType.Partially)
            {
                return new SolidColorBrush((Color)Application.Current.Resources["Color.Accent.Yellow.1"]);
            }

            return new SolidColorBrush((Color)Application.Current.Resources["Color.Text.6"]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}