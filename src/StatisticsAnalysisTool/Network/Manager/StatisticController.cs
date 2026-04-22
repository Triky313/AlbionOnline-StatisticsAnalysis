using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Serilog;
using SkiaSharp;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Manager;

public class StatisticController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly List<ValueType> _chartValueTypes =
    [
        ValueType.Fame,
        ValueType.Silver,
        ValueType.ReSpec,
        ValueType.FactionFame,
        ValueType.FactionPoints,
        ValueType.Might,
        ValueType.Favor
    ];

    private DateTime _lastChartUpdate;
    private DashboardStatistics _dashboardStatistics = new();

    public event Action OnAddValue;

    public StatisticController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;

        OnAddValue += UpdateRepairCostsUi;
    }

    #region Dashboard

    public void AddValue(ValueType valueType, double gainedValue)
    {
        if (!_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var now = DateTime.Now;

        _dashboardStatistics.Add(new DailyValues(valueType, gainedValue, now));

        if (_chartValueTypes.Contains(valueType))
        {
            _dashboardStatistics.Add(new HourlyValues(valueType, gainedValue, now));
            UpdateDailyChart();
        }

        OnAddValue?.Invoke();
    }

    public void UpdateDailyChart(bool forceUpdate = false)
    {
        if (!IsUpdateChartAllowed(forceUpdate))
        {
            return;
        }

        var selectedRange = _mainWindowViewModel.SelectedDashboardChartRange;
        if (selectedRange == null)
        {
            return;
        }

        var selectedSeriesFilters = (_mainWindowViewModel.DashboardChartSeriesFilters ?? [])
            .Where(x => x.IsSelected)
            .ToList();

        var chartBuckets = selectedRange.UseHourlyValues
            ? CreateHourlyBuckets(selectedRange.BucketCount)
            : CreateDailyBuckets(selectedRange.BucketCount);

        var xAxes = new[]
        {
            new Axis()
            {
                LabelsRotation = 15,
                Labels = chartBuckets.Select(x => x.Label).ToArray()
            }
        };

        if (selectedSeriesFilters.Count == 0)
        {
            _mainWindowViewModel.XAxesDashboardHourValues = xAxes;
            _mainWindowViewModel.SeriesDashboardHourValues = [];
            _lastChartUpdate = DateTime.Now;
            return;
        }

        var seriesCollection = new ObservableCollection<ISeries>();

        foreach (var selectedSeriesFilter in selectedSeriesFilters)
        {
            var valuesLookup = selectedRange.UseHourlyValues
                ? GetHourlyValuesLookup(selectedSeriesFilter.ValueType)
                : GetDailyValuesLookup(selectedSeriesFilter.ValueType);

            var points = new ObservableCollection<ObservablePoint>();

            for (var i = 0; i < chartBuckets.Count; i++)
            {
                var chartBucket = chartBuckets[i];
                var value = valuesLookup.GetValueOrDefault(chartBucket.Start);
                points.Add(new ObservablePoint(i, value));
            }

            var lineSeries = new LineSeries<ObservablePoint>
            {
                Name = selectedSeriesFilter.Name,
                Values = points,
                Fill = GetValueTypeBrush(selectedSeriesFilter.ValueType, true),
                Stroke = GetValueTypeBrush(selectedSeriesFilter.ValueType, false),
                GeometryStroke = GetValueTypeBrush(selectedSeriesFilter.ValueType, false),
                GeometryFill = GetValueTypeBrush(selectedSeriesFilter.ValueType, false),
                GeometrySize = 5,
                YToolTipLabelFormatter = chartPoint => chartPoint.Coordinate.PrimaryValue.ToChartTooltipNumberString()
            };

            seriesCollection.Add(lineSeries);
        }

        _mainWindowViewModel.XAxesDashboardHourValues = xAxes;
        _mainWindowViewModel.SeriesDashboardHourValues = seriesCollection;

        _lastChartUpdate = DateTime.Now;
    }

    private static List<ChartBucket> CreateHourlyBuckets(int bucketCount)
    {
        var buckets = new List<ChartBucket>(bucketCount);
        var currentHour = DateTime.Now;
        currentHour = new DateTime(currentHour.Year, currentHour.Month, currentHour.Day, currentHour.Hour, 0, 0);

        for (var i = bucketCount - 1; i >= 0; i--)
        {
            var start = currentHour.AddHours(-i);
            buckets.Add(new ChartBucket(start, start.ToString("dd.MM HH:mm", CultureInfo.CurrentCulture)));
        }

        return buckets;
    }

    private static List<ChartBucket> CreateDailyBuckets(int bucketCount)
    {
        var buckets = new List<ChartBucket>(bucketCount);
        var currentDay = DateTime.Now.Date;

        for (var i = bucketCount - 1; i >= 0; i--)
        {
            var start = currentDay.AddDays(-i);
            buckets.Add(new ChartBucket(start, start.ToString("d", CultureInfo.CurrentCulture)));
        }

        return buckets;
    }

    private Dictionary<DateTime, double> GetHourlyValuesLookup(ValueType valueType)
    {
        return (_dashboardStatistics.HourlyValues ?? [])
            .Where(x => x.ValueType == valueType)
            .GroupBy(x => x.Date)
            .ToDictionary(x => x.Key, x => x.Sum(v => v.Value));
    }

    private Dictionary<DateTime, double> GetDailyValuesLookup(ValueType valueType)
    {
        return (_dashboardStatistics.DailyValues ?? [])
            .Where(x => x.ValueType == valueType)
            .GroupBy(x => x.Date.Date)
            .ToDictionary(x => x.Key, x => x.Sum(v => v.Value));
    }

    private bool IsUpdateChartAllowed(bool forceUpdate)
    {
        if (forceUpdate)
        {
            return true;
        }

        return DateTime.Now > _lastChartUpdate.AddSeconds(20);
    }

    public static SolidColorPaint GetValueTypeBrush(ValueType valueType, bool transparent)
    {
        try
        {
            if (transparent)
            {
                var transparentBrush = (SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.Value.{valueType}.Transparent"];
                return new SolidColorPaint
                {
                    Color = new SKColor(transparentBrush.Color.R, transparentBrush.Color.G, transparentBrush.Color.B, transparentBrush.Color.A)
                };
            }

            var brush = (SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.Value.{valueType}"];
            return new SolidColorPaint
            {
                Color = new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A)
            };
        }
        catch
        {
            return new SolidColorPaint
            {
                Color = new SKColor(0, 0, 0, 0)
            };
        }
    }

    private sealed class ChartBucket
    {
        public ChartBucket(DateTime start, string label)
        {
            Start = start;
            Label = label;
        }

        public DateTime Start
        {
            get;
        }

        public string Label
        {
            get;
        }
    }

    #endregion

    #region Kill / Death infos

    public void SetKillsDeathsValues()
    {
        _mainWindowViewModel.DashboardBindings.KillsToday = _trackingController.EntityController.LocalUserData.KillsToday;
        _mainWindowViewModel.DashboardBindings.SoloKillsToday = _trackingController.EntityController.LocalUserData.SoloKillsToday;
        _mainWindowViewModel.DashboardBindings.DeathsToday = _trackingController.EntityController.LocalUserData.DeathsToday;
        _mainWindowViewModel.DashboardBindings.KillsThisWeek = _trackingController.EntityController.LocalUserData.KillsWeek;
        _mainWindowViewModel.DashboardBindings.SoloKillsThisWeek = _trackingController.EntityController.LocalUserData.SoloKillsWeek;
        _mainWindowViewModel.DashboardBindings.DeathsThisWeek = _trackingController.EntityController.LocalUserData.DeathsWeek;
        _mainWindowViewModel.DashboardBindings.KillsThisMonth = _trackingController.EntityController.LocalUserData.KillsMonth;
        _mainWindowViewModel.DashboardBindings.SoloKillsThisMonth = _trackingController.EntityController.LocalUserData.SoloKillsMonth;
        _mainWindowViewModel.DashboardBindings.DeathsThisMonth = _trackingController.EntityController.LocalUserData.DeathsMonth;

        _mainWindowViewModel.DashboardBindings.AverageItemPowerWhenKilling = _trackingController.EntityController.LocalUserData.AverageItemPowerWhenKilling;
        _mainWindowViewModel.DashboardBindings.AverageItemPowerOfTheKilledEnemies = _trackingController.EntityController.LocalUserData.AverageItemPowerOfTheKilledEnemies;
        _mainWindowViewModel.DashboardBindings.AverageItemPowerWhenDying = _trackingController.EntityController.LocalUserData.AverageItemPowerWhenDying;

        _mainWindowViewModel.DashboardBindings.LastUpdate = _trackingController.EntityController.LocalUserData.LastUpdate;
    }

    #endregion

    #region Repair costs stats

    public void UpdateRepairCostsUi()
    {
        var currentDate = DateTime.Now;

        if (_dashboardStatistics?.DailyValues == null)
        {
            _mainWindowViewModel.DashboardBindings.RepairCostsToday = 0;
            _mainWindowViewModel.DashboardBindings.RepairCostsLast7Days = 0;
            _mainWindowViewModel.DashboardBindings.RepairCostsLast30Days = 0;
            return;
        }

        _mainWindowViewModel.DashboardBindings.RepairCostsToday = _dashboardStatistics.DailyValues
            .Where(x => x is { ValueType: ValueType.RepairCosts }
                        && x.Date.Year == currentDate.Year
                        && x.Date.Month == currentDate.Month
                        && x.Date.Day == currentDate.Day)
            .Sum(x => FixPoint.FromFloatingPointValue(x.Value).IntegerValue);

        _mainWindowViewModel.DashboardBindings.RepairCostsLast7Days = _dashboardStatistics.DailyValues
            .Where(x => x is { ValueType: ValueType.RepairCosts }
                        && x.Date.Ticks > currentDate.AddDays(-7).Ticks)
            .Sum(x => FixPoint.FromFloatingPointValue(x.Value).IntegerValue);

        _mainWindowViewModel.DashboardBindings.RepairCostsLast30Days = _dashboardStatistics.DailyValues
            .Where(x => x is { ValueType: ValueType.RepairCosts }
                        && x.Date.Ticks > currentDate.AddDays(-30).Ticks)
            .Sum(x => FixPoint.FromFloatingPointValue(x.Value).IntegerValue);
    }

    #endregion

    #region Load / Save local file data

    public async System.Threading.Tasks.Task LoadFromFileAsync()
    {
        _dashboardStatistics = await FileController.LoadAsync<DashboardStatistics>(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.StatsFileName));

        _dashboardStatistics ??= new DashboardStatistics();
        _dashboardStatistics.DailyValues ??= [];
        _dashboardStatistics.HourlyValues ??= [];

        UpdateRepairCostsUi();
        UpdateDailyChart(true);
    }

    public async System.Threading.Tasks.Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));
        await FileController.SaveAsync(_dashboardStatistics, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.StatsFileName));
        Log.Information("Statistics saved");
    }

    #endregion
}
