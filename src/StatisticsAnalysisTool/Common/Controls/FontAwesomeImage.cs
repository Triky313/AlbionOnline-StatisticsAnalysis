using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using FontAwesomeImageBase = DSaladin.FontAwesome.WPF.FontAwesomeImage;

namespace StatisticsAnalysisTool.Common.Controls;

public sealed class FontAwesomeImage : FontAwesomeImageBase
{
    public static readonly DependencyProperty SpinProperty = DependencyProperty.Register(
        nameof(Spin),
        typeof(bool),
        typeof(FontAwesomeImage),
        new PropertyMetadata(false, OnSpinPropertyChanged));

    public static readonly DependencyProperty SpinDurationProperty = DependencyProperty.Register(
        nameof(SpinDuration),
        typeof(double),
        typeof(FontAwesomeImage),
        new PropertyMetadata(4d, OnSpinPropertyChanged),
        value => (double) value > 0);

    private readonly RotateTransform _spinTransform = new();

    public FontAwesomeImage()
    {
        RenderTransformOrigin = new Point(0.5, 0.5);
        RenderTransform = _spinTransform;
    }

    public bool Spin
    {
        get => (bool) GetValue(SpinProperty);
        set => SetValue(SpinProperty, value);
    }

    public double SpinDuration
    {
        get => (double) GetValue(SpinDurationProperty);
        set => SetValue(SpinDurationProperty, value);
    }

    private static void OnSpinPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
    {
        ((FontAwesomeImage) dependencyObject).UpdateSpinAnimation();
    }

    private void UpdateSpinAnimation()
    {
        _spinTransform.BeginAnimation(RotateTransform.AngleProperty, null);
        _spinTransform.Angle = 0;

        if (!Spin)
        {
            return;
        }

        var animation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(SpinDuration))
        {
            RepeatBehavior = RepeatBehavior.Forever
        };

        _spinTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
    }
}