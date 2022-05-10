using System;
using System.Windows;
using System.Windows.Controls;

namespace StatisticsAnalysisTool.Common;

public class ScrollViewerExtensions
{
    public static readonly DependencyProperty AlwaysScrollToEndProperty = 
        DependencyProperty.RegisterAttached("AlwaysScrollToEnd", typeof(bool), typeof(ScrollViewerExtensions), new PropertyMetadata(false, AlwaysScrollToEndChanged));
    private static bool _autoScroll;

    private static void AlwaysScrollToEndChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is ScrollViewer scroll)
        {
            var alwaysScrollToEnd = (e.NewValue != null) && (bool)e.NewValue;
            if (alwaysScrollToEnd)
            {
                scroll.ScrollToEnd();
                scroll.ScrollChanged += ScrollChanged;
            }
            else
            {
                scroll.ScrollChanged -= ScrollChanged;
            }
        }
        else { throw new InvalidOperationException("The attached AlwaysScrollToEnd property can only be applied to ScrollViewer instances."); }
    }

    public static bool GetAlwaysScrollToEnd(ScrollViewer scroll)
    {
        if (scroll == null) { throw new ArgumentNullException("scroll"); }
        return (bool)scroll.GetValue(AlwaysScrollToEndProperty);
    }

    public static void SetAlwaysScrollToEnd(ScrollViewer scroll, bool alwaysScrollToEnd)
    {
        if (scroll == null) { throw new ArgumentNullException("scroll"); }
        scroll.SetValue(AlwaysScrollToEndProperty, alwaysScrollToEnd);
    }

    private static void ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer scroll)
        {
            throw new InvalidOperationException("The attached AlwaysScrollToEnd property can only be applied to ScrollViewer instances.");
        }
        
        if (e.ExtentHeightChange == 0)
        {
            _autoScroll = Math.Abs(scroll.VerticalOffset - scroll.ScrollableHeight) < 0;
        }
        
        if (_autoScroll && e.ExtentHeightChange != 0)
        {
            scroll.ScrollToVerticalOffset(scroll.ExtentHeight);
        }
    }
}