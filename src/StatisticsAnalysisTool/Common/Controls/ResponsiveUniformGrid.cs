using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace StatisticsAnalysisTool.Common.Controls;

public sealed class ResponsiveUniformGrid : UniformGrid
{
    protected override Size MeasureOverride(Size availableSize)
    {
        var visibleChildCount = 0;
        var widestChild = 0d;
        var childAvailableSize = new Size(double.PositiveInfinity, availableSize.Height);

        foreach (UIElement child in InternalChildren)
        {
            if (child.Visibility == Visibility.Collapsed)
            {
                continue;
            }

            child.Measure(childAvailableSize);
            widestChild = Math.Max(widestChild, child.DesiredSize.Width);
            visibleChildCount++;
        }

        var columns = Math.Max(1, visibleChildCount);
        if (!double.IsPositiveInfinity(availableSize.Width) && widestChild > 0)
        {
            columns = (int) Math.Max(1, Math.Min(columns, Math.Floor(availableSize.Width / widestChild)));
        }

        SetCurrentValue(ColumnsProperty, columns);
        return base.MeasureOverride(availableSize);
    }
}