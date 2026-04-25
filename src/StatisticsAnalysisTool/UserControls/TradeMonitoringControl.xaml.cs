using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for TradeMonitoringControl.xaml
/// </summary>
public partial class TradeMonitoringControl
{
    private const float ProfitByTimeOfDayHeatmapLeftPadding = 72f;
    private const float ProfitByTimeOfDayHeatmapTopPadding = 24f;
    private const float ProfitByTimeOfDayHeatmapRightPadding = 14f;
    private const float ProfitByTimeOfDayHeatmapBottomPadding = 38f;
    private const float ProfitByTimeOfDayHeatmapCellGap = 4f;
    private const float ProfitByTimeOfDayHeatmapLabelTextSize = 12f;
    private const float ProfitByTimeOfDayHeatmapCellCornerRadius = 4f;
    private const int ProfitByTimeOfDayHeatmapHours = 24;
    private const int ProfitByTimeOfDayHeatmapDays = 7;

    private TradeMonitoringBindings _subscribedTradeMonitoringBindings;

    public TradeMonitoringControl()
    {
        InitializeComponent();
        DataContextChanged += TradeMonitoringControl_OnDataContextChanged;
    }

    public async Task DeleteSelectedTradesAsync()
    {
        var dialog = new DialogWindow(LocalizationController.Translation("DELETE_SELECTED_TRADES"), LocalizationController.Translation("SURE_YOU_WANT_TO_DELETE_SELECTED_TRADES"));
        var dialogResult = dialog.ShowDialog();

        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();

        if (mainWindowViewModel == null)
        {
            return;
        }

        if (dialogResult is true)
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();

            var selectedTrades = mainWindowViewModel.TradeMonitoringBindings.TradeCollectionView?.Cast<Trade.Trade>()
                .ToList()
                .Where(x => x?.IsSelectedForDeletion ?? false)
                .Select(x => x.Id);

            mainWindowViewModel.TradeMonitoringBindings.IsDeleteTradesButtonEnabled = false;
            await trackingController?.TradeController?.RemoveTradesByIdsAsync(selectedTrades)!;
            mainWindowViewModel.TradeMonitoringBindings.IsDeleteTradesButtonEnabled = true;
        }
    }

    #region Ui events

    private void OpenMailMonitoringPopup_MouseEnter(object sender, MouseEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm.TradeMonitoringBindings.IsTradeMonitoringPopupVisible = Visibility.Visible;
    }

    private void CloseMailMonitoringPopup_MouseLeave(object sender, MouseEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm.TradeMonitoringBindings.IsTradeMonitoringPopupVisible = Visibility.Collapsed;
    }

    private async void BtnDeleteSelectedMails_Click(object sender, RoutedEventArgs e)
    {
        await DeleteSelectedTradesAsync();
        _isSelectAllActive = !_isSelectAllActive;
    }

    private bool _isSelectAllActive;

    private void BtnSelectSwitchAllMails_Click(object sender, RoutedEventArgs e)
    {
        if ((MainWindowViewModel) DataContext is not { TradeMonitoringBindings.Trades: { } } mainWindowViewModel)
        {
            return;
        }

        foreach (var trade in mainWindowViewModel.TradeMonitoringBindings.TradeCollectionView)
        {
            ((Trade.Trade) trade).IsSelectedForDeletion = !_isSelectAllActive;
        }

        _isSelectAllActive = !_isSelectAllActive;
    }

    private async void SearchText_TextChanged(object sender, TextChangedEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        await vm.TradeMonitoringBindings.UpdateFilteredTradesAsync();
    }

    private async void DatePicker_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        await vm.TradeMonitoringBindings.UpdateFilteredTradesAsync();
    }

    private async void FilterSelection_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        await vm.TradeMonitoringBindings.UpdateFilteredTradesAsync();
    }

    private async void ProfitOverTimeAggregation_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (sender is ComboBox { SelectedValue: TradeProfitTimeAggregation aggregation })
        {
            vm.TradeMonitoringBindings.SelectedProfitOverTimeAggregation = aggregation;
        }

        await vm.TradeMonitoringBindings.UpdateProfitOverTimeChartAsync();
    }

    private void ProfitByTimeOfDayChartMode_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (sender is ComboBox { SelectedValue: TradeTimeOfDayChartMode chartMode })
        {
            vm.TradeMonitoringBindings.SelectedProfitByTimeOfDayChartMode = chartMode;
        }

        vm.TradeMonitoringBindings.RefreshProfitByTimeOfDayPresentation();
        ProfitByTimeOfDayHeatmap?.InvalidateVisual();
    }

    private void ProfitByTimeOfDayMetric_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (sender is ComboBox { SelectedValue: TradeTimeOfDayMetric metric })
        {
            vm.TradeMonitoringBindings.SelectedProfitByTimeOfDayMetric = metric;
        }

        vm.TradeMonitoringBindings.RefreshProfitByTimeOfDayPresentation();
        ProfitByTimeOfDayHeatmap?.InvalidateVisual();
    }

    private void FilterReset_MouseUp(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm.TradeMonitoringBindings?.ItemFilterReset();
    }

    private void TopItemRankingList_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (TopItemRankingScrollViewer == null)
        {
            return;
        }

        var scrollSteps = Math.Max(1, Math.Abs(e.Delta) / Mouse.MouseWheelDeltaForOneLine);

        for (var i = 0; i < scrollSteps; i++)
        {
            if (e.Delta > 0)
            {
                TopItemRankingScrollViewer.LineUp();
            }
            else
            {
                TopItemRankingScrollViewer.LineDown();
            }
        }

        e.Handled = true;
    }

    private void DiagramsChart_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (DiagramsScrollViewer == null)
        {
            return;
        }

        var scrollSteps = Math.Max(1, Math.Abs(e.Delta) / Mouse.MouseWheelDeltaForOneLine);

        for (var i = 0; i < scrollSteps; i++)
        {
            if (e.Delta > 0)
            {
                DiagramsScrollViewer.LineUp();
            }
            else
            {
                DiagramsScrollViewer.LineDown();
            }
        }

        e.Handled = true;
    }

    private void TradeMonitoringControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_subscribedTradeMonitoringBindings != null)
        {
            _subscribedTradeMonitoringBindings.PropertyChanged -= TradeMonitoringBindings_OnPropertyChanged;
        }

        _subscribedTradeMonitoringBindings = (e.NewValue as MainWindowViewModel)?.TradeMonitoringBindings;
        if (_subscribedTradeMonitoringBindings != null)
        {
            _subscribedTradeMonitoringBindings.PropertyChanged += TradeMonitoringBindings_OnPropertyChanged;
        }

        ProfitByTimeOfDayHeatmap?.InvalidateVisual();
    }

    private void TradeMonitoringBindings_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.PropertyName))
        {
            return;
        }

        if (!e.PropertyName.StartsWith("ProfitByTimeOfDay", StringComparison.Ordinal))
        {
            return;
        }

        Dispatcher.InvokeAsync(() => ProfitByTimeOfDayHeatmap?.InvalidateVisual());
    }

    private void ProfitByTimeOfDayHeatmap_OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        var heatmapPoints = vm.TradeMonitoringBindings.ProfitByTimeOfDayHeatmapPoints;
        if (heatmapPoints == null || heatmapPoints.Count == 0)
        {
            return;
        }

        var layout = BuildProfitByTimeOfDayHeatmapLayout(e.Info.Width, e.Info.Height);
        if (!layout.IsValid)
        {
            return;
        }

        var neutralColor = GetSkColor("SolidColorBrush.Background.4", new SKColor(90, 90, 90, 255));
        var positiveColor = GetSkColor("SolidColorBrush.Accent.Green.3", new SKColor(65, 186, 116, 255));
        var negativeColor = GetSkColor("SolidColorBrush.Accent.Red.3", new SKColor(207, 79, 87, 255));
        var strokeColor = GetSkColor("SolidColorBrush.Background.2", new SKColor(40, 40, 40, 255));
        var textColor = GetSkColor("SolidColorBrush.Text.1", new SKColor(230, 230, 230, 255));

        var pointLookup = heatmapPoints.ToDictionary(
            point => (point.DayOfWeek ?? DayOfWeek.Monday, point.Hour),
            point => point);

        var metric = vm.TradeMonitoringBindings.SelectedProfitByTimeOfDayMetric;
        var metricValues = heatmapPoints.Select(point => point.GetMetricValue(metric)).ToList();
        var maxPositive = metricValues.Where(value => value > 0d).DefaultIfEmpty(0d).Max();
        var maxNegative = metricValues.Where(value => value < 0d).Select(Math.Abs).DefaultIfEmpty(0d).Max();

        using var fillPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        using var strokePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = strokeColor,
            StrokeWidth = 1f
        };
        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            Color = textColor
        };
        using var textFont = new SKFont
        {
            Size = ProfitByTimeOfDayHeatmapLabelTextSize
        };

        for (var dayIndex = 0; dayIndex < ProfitByTimeOfDayHeatmapDays; dayIndex++)
        {
            var dayOfWeek = GetOrderedDayOfWeek(dayIndex);

            for (var hour = 0; hour < ProfitByTimeOfDayHeatmapHours; hour++)
            {
                if (!pointLookup.TryGetValue((dayOfWeek, hour), out var point))
                {
                    continue;
                }

                var rect = layout.GetCellRect(dayIndex, hour);
                var metricValue = point.GetMetricValue(metric);
                fillPaint.Color = ResolveHeatmapCellColor(metricValue, maxPositive, maxNegative, neutralColor, positiveColor, negativeColor);

                canvas.DrawRoundRect(rect, ProfitByTimeOfDayHeatmapCellCornerRadius, ProfitByTimeOfDayHeatmapCellCornerRadius, fillPaint);
                canvas.DrawRoundRect(rect, ProfitByTimeOfDayHeatmapCellCornerRadius, ProfitByTimeOfDayHeatmapCellCornerRadius, strokePaint);
            }
        }

        DrawProfitByTimeOfDayHeatmapLabels(canvas, layout, textPaint, textFont);
    }

    private void ProfitByTimeOfDayHeatmap_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (ProfitByTimeOfDayHeatmap == null || DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (!TryGetProfitByTimeOfDayHeatmapCell(e.GetPosition(ProfitByTimeOfDayHeatmap), ProfitByTimeOfDayHeatmap.RenderSize, out var dayOfWeek, out var hour))
        {
            ProfitByTimeOfDayHeatmap.ToolTip = null;
            return;
        }

        var point = vm.TradeMonitoringBindings.GetProfitByTimeOfDayHeatmapPoint(dayOfWeek, hour);
        if (point == null)
        {
            ProfitByTimeOfDayHeatmap.ToolTip = null;
            return;
        }

        ProfitByTimeOfDayHeatmap.ToolTip = vm.TradeMonitoringBindings.FormatProfitByTimeOfDayTooltip(point);
    }

    private void ProfitByTimeOfDayHeatmap_OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (ProfitByTimeOfDayHeatmap == null)
        {
            return;
        }

        ProfitByTimeOfDayHeatmap.ToolTip = null;
    }

    private static ProfitByTimeOfDayHeatmapLayout BuildProfitByTimeOfDayHeatmapLayout(float width, float height)
    {
        var plotWidth = width - ProfitByTimeOfDayHeatmapLeftPadding - ProfitByTimeOfDayHeatmapRightPadding;
        var plotHeight = height - ProfitByTimeOfDayHeatmapTopPadding - ProfitByTimeOfDayHeatmapBottomPadding;

        if (plotWidth <= 0f || plotHeight <= 0f)
        {
            return ProfitByTimeOfDayHeatmapLayout.Invalid;
        }

        var cellWidth = (plotWidth - (ProfitByTimeOfDayHeatmapHours - 1) * ProfitByTimeOfDayHeatmapCellGap) / ProfitByTimeOfDayHeatmapHours;
        var cellHeight = (plotHeight - (ProfitByTimeOfDayHeatmapDays - 1) * ProfitByTimeOfDayHeatmapCellGap) / ProfitByTimeOfDayHeatmapDays;

        return cellWidth <= 0f || cellHeight <= 0f
            ? ProfitByTimeOfDayHeatmapLayout.Invalid
            : new ProfitByTimeOfDayHeatmapLayout(
                ProfitByTimeOfDayHeatmapLeftPadding,
                ProfitByTimeOfDayHeatmapTopPadding,
                cellWidth,
                cellHeight,
                ProfitByTimeOfDayHeatmapCellGap);
    }

    private static void DrawProfitByTimeOfDayHeatmapLabels(SKCanvas canvas, ProfitByTimeOfDayHeatmapLayout layout, SKPaint textPaint, SKFont textFont)
    {
        var fontMetrics = textFont.Metrics;
        var xLabelY = layout.Top + layout.TotalHeight + ProfitByTimeOfDayHeatmapLabelTextSize + 8f;

        for (var hour = 0; hour < ProfitByTimeOfDayHeatmapHours; hour++)
        {
            var rect = layout.GetCellRect(0, hour);
            canvas.DrawText(hour.ToString("00", CultureInfo.CurrentCulture), rect.MidX, xLabelY, SKTextAlign.Center, textFont, textPaint);
        }

        var textOffset = (fontMetrics.Descent + fontMetrics.Ascent) / 2f;

        for (var dayIndex = 0; dayIndex < ProfitByTimeOfDayHeatmapDays; dayIndex++)
        {
            var dayRect = layout.GetCellRect(dayIndex, 0);
            var dayLabel = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(GetOrderedDayOfWeek(dayIndex));
            canvas.DrawText(dayLabel, layout.Left - 8f, dayRect.MidY - textOffset, SKTextAlign.Right, textFont, textPaint);
        }
    }

    private static bool TryGetProfitByTimeOfDayHeatmapCell(Point mousePosition, Size renderSize, out DayOfWeek dayOfWeek, out int hour)
    {
        var layout = BuildProfitByTimeOfDayHeatmapLayout((float) renderSize.Width, (float) renderSize.Height);
        if (!layout.IsValid)
        {
            dayOfWeek = DayOfWeek.Monday;
            hour = 0;
            return false;
        }

        for (var dayIndex = 0; dayIndex < ProfitByTimeOfDayHeatmapDays; dayIndex++)
        {
            for (var hourIndex = 0; hourIndex < ProfitByTimeOfDayHeatmapHours; hourIndex++)
            {
                var rect = layout.GetCellRect(dayIndex, hourIndex);
                if (mousePosition.X < rect.Left || mousePosition.X > rect.Right || mousePosition.Y < rect.Top || mousePosition.Y > rect.Bottom)
                {
                    continue;
                }

                dayOfWeek = GetOrderedDayOfWeek(dayIndex);
                hour = hourIndex;
                return true;
            }
        }

        dayOfWeek = DayOfWeek.Monday;
        hour = 0;
        return false;
    }

    private static DayOfWeek GetOrderedDayOfWeek(int dayIndex)
    {
        return dayIndex switch
        {
            0 => DayOfWeek.Monday,
            1 => DayOfWeek.Tuesday,
            2 => DayOfWeek.Wednesday,
            3 => DayOfWeek.Thursday,
            4 => DayOfWeek.Friday,
            5 => DayOfWeek.Saturday,
            _ => DayOfWeek.Sunday
        };
    }

    private static SKColor ResolveHeatmapCellColor(double value, double maxPositive, double maxNegative, SKColor neutralColor, SKColor positiveColor, SKColor negativeColor)
    {
        if (Math.Abs(value) <= double.Epsilon)
        {
            return neutralColor;
        }

        if (value > 0d && maxPositive > 0d)
        {
            return BlendHeatmapColors(neutralColor, positiveColor, value / maxPositive);
        }

        if (value < 0d && maxNegative > 0d)
        {
            return BlendHeatmapColors(neutralColor, negativeColor, Math.Abs(value) / maxNegative);
        }

        return neutralColor;
    }

    private static SKColor BlendHeatmapColors(SKColor fromColor, SKColor toColor, double intensity)
    {
        var normalizedIntensity = (float) Math.Clamp(intensity, 0d, 1d);
        var effectiveIntensity = 0.18f + 0.82f * normalizedIntensity;

        return new SKColor(
            (byte) (fromColor.Red + (toColor.Red - fromColor.Red) * effectiveIntensity),
            (byte) (fromColor.Green + (toColor.Green - fromColor.Green) * effectiveIntensity),
            (byte) (fromColor.Blue + (toColor.Blue - fromColor.Blue) * effectiveIntensity),
            255);
    }

    private static SKColor GetSkColor(string resourceKey, SKColor fallbackColor)
    {
        if (Application.Current?.Resources[resourceKey] is SolidColorBrush brush)
        {
            return new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
        }

        return fallbackColor;
    }

    private readonly record struct ProfitByTimeOfDayHeatmapLayout(float Left, float Top, float CellWidth, float CellHeight, float Gap)
    {
        public static ProfitByTimeOfDayHeatmapLayout Invalid
        {
            get;
        } = new(float.NaN, float.NaN, float.NaN, float.NaN, float.NaN);

        public bool IsValid
        {
            get
            {
                return !float.IsNaN(CellWidth) && CellWidth > 0f && !float.IsNaN(CellHeight) && CellHeight > 0f;
            }
        }

        public float TotalHeight => ProfitByTimeOfDayHeatmapDays * CellHeight + (ProfitByTimeOfDayHeatmapDays - 1) * Gap;

        public SKRect GetCellRect(int dayIndex, int hourIndex)
        {
            var left = Left + hourIndex * (CellWidth + Gap);
            var top = Top + dayIndex * (CellHeight + Gap);
            return new SKRect(left, top, left + CellWidth, top + CellHeight);
        }
    }

    #endregion
}