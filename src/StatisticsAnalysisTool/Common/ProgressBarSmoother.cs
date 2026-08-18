using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace StatisticsAnalysisTool.Common;

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
        if (d is not ProgressBar progressBar)
        {
            return;
        }

        var currentValue = NormalizeValue(progressBar.Value, progressBar.Minimum, progressBar.Maximum);
        var targetValue = NormalizeValue((double) e.NewValue, progressBar.Minimum, progressBar.Maximum);
        var animation = new DoubleAnimation(currentValue, targetValue, TimeSpan.FromMilliseconds(250));
        progressBar.BeginAnimation(RangeBase.ValueProperty, animation, HandoffBehavior.SnapshotAndReplace);
    }

    private static double NormalizeValue(double value, double minimum, double maximum)
    {
        return double.IsNaN(value) || double.IsInfinity(value) ? minimum : Math.Clamp(value, minimum, maximum);
    }
}