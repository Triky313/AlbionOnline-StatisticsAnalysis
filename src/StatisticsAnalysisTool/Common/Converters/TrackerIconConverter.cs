using StatisticsAnalysisTool.Enumerations;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace StatisticsAnalysisTool.Common.Converters
{
    public class TrackerIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new SolidColorBrush((Color)Application.Current.Resources["Tracking.Off"]);
            }

            if ((TrackingIconType)value == TrackingIconType.On)
            {
                return new SolidColorBrush((Color)Application.Current.Resources["Tracking.On"]);
            }

            if ((TrackingIconType)value == TrackingIconType.Partially)
            {
                return new SolidColorBrush((Color)Application.Current.Resources["Tracking.Partially"]);
            }

            return new SolidColorBrush((Color)Application.Current.Resources["Tracking.Off"]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}