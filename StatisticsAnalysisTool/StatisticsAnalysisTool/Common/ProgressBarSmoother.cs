using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace StatisticsAnalysisTool.Common
{
    // https://stackoverflow.com/questions/14485818/how-to-update-a-progress-bar-so-it-increases-smoothly
    public class ProgressBarSmoother
    {
        public static readonly DependencyProperty SmoothValueProperty =
            DependencyProperty.RegisterAttached("SmoothValue", typeof(double), typeof(ProgressBarSmoother), new PropertyMetadata(0.0, Changing));

        public static double GetSmoothValue(DependencyObject obj)
        {
            return (double) obj.GetValue(SmoothValueProperty);
        }

        public static void SetSmoothValue(DependencyObject obj, double value)
        {
            obj.SetValue(SmoothValueProperty, value);
        }

        private static void Changing(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (double.IsNaN((double) e.OldValue) || double.IsNaN((double) e.NewValue))
            {
                return;
            }

            var anim = new DoubleAnimation((double)e.OldValue, (double)e.NewValue, new TimeSpan(0, 0, 0, 0, 250));
            (d as ProgressBar)?.BeginAnimation(RangeBase.ValueProperty, anim, HandoffBehavior.Compose);
        }
    }
}