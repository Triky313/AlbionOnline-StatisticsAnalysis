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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CityFaction = StatisticsAnalysisTool.Enumerations.CityFaction;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Manager;

public class StatisticController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly List<ValueType> _valueTypes = new()
    {
        ValueType.Fame, ValueType.Silver, ValueType.ReSpec, ValueType.FactionFame, ValueType.FactionPoints, ValueType.Might, ValueType.Favor, ValueType.RepairCosts, ValueType.GatheringValue
    };

    private DateTime _lastChartUpdate;
    private DashboardStatistics _dashboardStatistics = new();

    public event Action OnAddValue;

    public StatisticController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
        InitStartHourValues();

        OnAddValue += UpdateRepairCostsUi;
        OnAddValue += UpdateLifetimeStatsUi;
    }

    #region Dashboard

    private void InitStartHourValues()
    {
        foreach (var valueType in _valueTypes)
        {
            var dashboardHourObject = new DashboardHourObject()
            {
                Type = valueType
            };

            var dateTimeNow = DateTime.Now;
            for (var i = 0; i < 23; i++)
            {
                dashboardHourObject.HourValues.Add(new DashboardHourValues()
                {
                    Date = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day, i, 0, 0),
                    Value = 0f
                });
            }
        }
    }

    public void AddValue(ValueType valueType, double gainedValue)
    {
        if (!_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        _dashboardStatistics.Add(new DailyValues(valueType, gainedValue, DateTime.Now));
        OnAddValue?.Invoke();
    }

    public void AddActiveTime(double seconds)
    {
        _dashboardStatistics.Add(new DailyValues(ValueType.ActiveTime, seconds, DateTime.Now));
    }

    private void UpdateDailyChart(ObservableCollection<DashboardHourObject> stats)
    {
        if (!IsUpdateChartAllowed())
        {
            return;
        }

        var date = new List<string>();
        var seriesCollection = new ObservableCollection<ISeries>();
        var xAxes = new ObservableCollection<Axis>();

        foreach (var dashboardHourObject in stats)
        {
            var amount = new ObservableCollection<ObservablePoint>();

            var counter = 0;
            foreach (var data in dashboardHourObject.HourValues.OrderBy(x => x.Date).ToList())
            {
                if (!date.Exists(x => x.Contains(data.Date.ToString("g", CultureInfo.CurrentCulture))))
                {
                    date.Add(data.Date.ToString("g", CultureInfo.CurrentCulture));
                }

                amount.Add(new ObservablePoint(counter++, data.Value));
            }

            var lineSeries = new LineSeries<ObservablePoint>
            {
                Name = dashboardHourObject.Type.ToString(),
                Values = amount,
                Fill = new SolidColorPaint
                {
                    Color = new SKColor(0, 0, 0, 0)
                },
                Stroke = GetValueTypeBrush(dashboardHourObject.Type, false),
                GeometryStroke = GetValueTypeBrush(dashboardHourObject.Type, false),
                GeometryFill = GetValueTypeBrush(dashboardHourObject.Type, true),
                GeometrySize = 5
            };

            seriesCollection.Add(lineSeries);
        }

        xAxes.Add(new Axis()
        {
            LabelsRotation = 15,
            Labels = date,
            Labeler = value => new DateTime((long) value).ToString(CultureInfo.CurrentCulture),
            UnitWidth = TimeSpan.FromHours(1).Ticks
        });

        _mainWindowViewModel.XAxesDashboardHourValues = xAxes.ToArray();
        _mainWindowViewModel.SeriesDashboardHourValues = seriesCollection;

        _lastChartUpdate = DateTime.Now;

        // TODO: Bar chart for Fame, ReSpec etc.
        //var series = new ISeries[]
        //{
        //    new ColumnSeries<double>
        //    {
        //        Values = new double[] {2, 5, 4}
        //    },
        //    new ColumnSeries<double>
        //    {
        //        Values = new double[] {2, 5, 4}
        //    }
        //};

        //_mainWindowViewModel.SeriesDashboardFameDailyValues = series;

        //var axis = new[]
        //{
        //    new Axis
        //    {
        //        Labels = new [] {"08.04.2022", "07.04.2022", "06.04.2022"},
        //        LabelsRotation = 15
        //    }
        //};

        //_mainWindowViewModel.XAxesDashboardFameDailyValues = axis;
    }

    private bool IsUpdateChartAllowed()
    {
        return DateTime.Now > _lastChartUpdate.AddSeconds(20);
    }

    public static SolidColorPaint GetValueTypeBrush(ValueType valueType, bool transparent)
    {
        try
        {
            if (transparent)
            {
                var scbt = (SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.Value.{valueType}.Transparent"];
                return new SolidColorPaint
                {
                    Color = new SKColor(scbt.Color.R, scbt.Color.G, scbt.Color.B, scbt.Color.A)
                };
            }

            var scb = (SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.Value.{valueType}"];
            return new SolidColorPaint
            {
                Color = new SKColor(scb.Color.R, scb.Color.G, scb.Color.B, scb.Color.A)
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

    private class DashboardHourObject
    {
        public ValueType Type { get; init; }
        public ObservableCollection<DashboardHourValues> HourValues { get; } = new();
    }

    private class DashboardHourValues
    {
        public double Value { get; set; }
        public DateTime Date { get; init; }
        public int Hour => Date.Hour;
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

    public void UpdateLifetimeStatsUi()
    {
        if (_dashboardStatistics?.DailyValues == null)
        {
            return;
        }

        var stats = _mainWindowViewModel.DashboardBindings.LifetimeStats;
        var bindings = _mainWindowViewModel.DashboardBindings;
        var now = DateTime.Now;

        var todayStart = now.Date;
        var thisWeekStart = now.Date.AddDays(-(int) now.DayOfWeek == 0 ? 6 : (int) now.DayOfWeek - 1);
        if (thisWeekStart > now.Date) thisWeekStart = thisWeekStart.AddDays(-7);
        var lastWeekStart = thisWeekStart.AddDays(-7);
        var lastWeekEnd = thisWeekStart;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);
        var lastMonthEnd = thisMonthStart;
        var thisYearStart = new DateTime(now.Year, 1, 1);

        // Today
        var todayHours = SumActiveHours(todayStart, now);
        stats.FameToday = SumValue(ValueType.Fame, todayStart, now);
        stats.SilverToday = SumValue(ValueType.Silver, todayStart, now);
        stats.ReSpecToday = SumValue(ValueType.ReSpec, todayStart, now);
        stats.MightToday = SumValue(ValueType.Might, todayStart, now);
        stats.FavorToday = SumValue(ValueType.Favor, todayStart, now);
        stats.FactionPointsToday = SumValue(ValueType.FactionPoints, todayStart, now);
        stats.FameTodayPerHour = PerHour(stats.FameToday, todayHours);
        stats.SilverTodayPerHour = PerHour(stats.SilverToday, todayHours);
        stats.ReSpecTodayPerHour = PerHour(stats.ReSpecToday, todayHours);
        stats.MightTodayPerHour = PerHour(stats.MightToday, todayHours);
        stats.FavorTodayPerHour = PerHour(stats.FavorToday, todayHours);
        stats.FactionPointsTodayPerHour = PerHour(stats.FactionPointsToday, todayHours);
        stats.GatheringValueToday = SumValue(ValueType.GatheringValue, todayStart, now);
        stats.GatheringValueTodayPerHour = PerHour(stats.GatheringValueToday, todayHours);

        // This Week
        var thisWeekHours = SumActiveHours(thisWeekStart, now);
        stats.FameThisWeek = SumValue(ValueType.Fame, thisWeekStart, now);
        stats.SilverThisWeek = SumValue(ValueType.Silver, thisWeekStart, now);
        stats.ReSpecThisWeek = SumValue(ValueType.ReSpec, thisWeekStart, now);
        stats.MightThisWeek = SumValue(ValueType.Might, thisWeekStart, now);
        stats.FavorThisWeek = SumValue(ValueType.Favor, thisWeekStart, now);
        stats.FactionPointsThisWeek = SumValue(ValueType.FactionPoints, thisWeekStart, now);
        stats.FameThisWeekPerHour = PerHour(stats.FameThisWeek, thisWeekHours);
        stats.SilverThisWeekPerHour = PerHour(stats.SilverThisWeek, thisWeekHours);
        stats.ReSpecThisWeekPerHour = PerHour(stats.ReSpecThisWeek, thisWeekHours);
        stats.MightThisWeekPerHour = PerHour(stats.MightThisWeek, thisWeekHours);
        stats.FavorThisWeekPerHour = PerHour(stats.FavorThisWeek, thisWeekHours);
        stats.FactionPointsThisWeekPerHour = PerHour(stats.FactionPointsThisWeek, thisWeekHours);
        stats.GatheringValueThisWeek = SumValue(ValueType.GatheringValue, thisWeekStart, now);
        stats.GatheringValueThisWeekPerHour = PerHour(stats.GatheringValueThisWeek, thisWeekHours);

        // Last Week
        var lastWeekHours = SumActiveHours(lastWeekStart, lastWeekEnd);
        stats.FameLastWeek = SumValue(ValueType.Fame, lastWeekStart, lastWeekEnd);
        stats.SilverLastWeek = SumValue(ValueType.Silver, lastWeekStart, lastWeekEnd);
        stats.ReSpecLastWeek = SumValue(ValueType.ReSpec, lastWeekStart, lastWeekEnd);
        stats.MightLastWeek = SumValue(ValueType.Might, lastWeekStart, lastWeekEnd);
        stats.FavorLastWeek = SumValue(ValueType.Favor, lastWeekStart, lastWeekEnd);
        stats.FactionPointsLastWeek = SumValue(ValueType.FactionPoints, lastWeekStart, lastWeekEnd);
        stats.FameLastWeekPerHour = PerHour(stats.FameLastWeek, lastWeekHours);
        stats.SilverLastWeekPerHour = PerHour(stats.SilverLastWeek, lastWeekHours);
        stats.ReSpecLastWeekPerHour = PerHour(stats.ReSpecLastWeek, lastWeekHours);
        stats.MightLastWeekPerHour = PerHour(stats.MightLastWeek, lastWeekHours);
        stats.FavorLastWeekPerHour = PerHour(stats.FavorLastWeek, lastWeekHours);
        stats.FactionPointsLastWeekPerHour = PerHour(stats.FactionPointsLastWeek, lastWeekHours);
        stats.GatheringValueLastWeek = SumValue(ValueType.GatheringValue, lastWeekStart, lastWeekEnd);
        stats.GatheringValueLastWeekPerHour = PerHour(stats.GatheringValueLastWeek, lastWeekHours);

        // This Month
        var thisMonthHours = SumActiveHours(thisMonthStart, now);
        stats.FameThisMonth = SumValue(ValueType.Fame, thisMonthStart, now);
        stats.SilverThisMonth = SumValue(ValueType.Silver, thisMonthStart, now);
        stats.ReSpecThisMonth = SumValue(ValueType.ReSpec, thisMonthStart, now);
        stats.MightThisMonth = SumValue(ValueType.Might, thisMonthStart, now);
        stats.FavorThisMonth = SumValue(ValueType.Favor, thisMonthStart, now);
        stats.FactionPointsThisMonth = SumValue(ValueType.FactionPoints, thisMonthStart, now);
        stats.FameThisMonthPerHour = PerHour(stats.FameThisMonth, thisMonthHours);
        stats.SilverThisMonthPerHour = PerHour(stats.SilverThisMonth, thisMonthHours);
        stats.ReSpecThisMonthPerHour = PerHour(stats.ReSpecThisMonth, thisMonthHours);
        stats.MightThisMonthPerHour = PerHour(stats.MightThisMonth, thisMonthHours);
        stats.FavorThisMonthPerHour = PerHour(stats.FavorThisMonth, thisMonthHours);
        stats.FactionPointsThisMonthPerHour = PerHour(stats.FactionPointsThisMonth, thisMonthHours);
        stats.GatheringValueThisMonth = SumValue(ValueType.GatheringValue, thisMonthStart, now);
        stats.GatheringValueThisMonthPerHour = PerHour(stats.GatheringValueThisMonth, thisMonthHours);

        // Last Month
        var lastMonthHours = SumActiveHours(lastMonthStart, lastMonthEnd);
        stats.FameLastMonth = SumValue(ValueType.Fame, lastMonthStart, lastMonthEnd);
        stats.SilverLastMonth = SumValue(ValueType.Silver, lastMonthStart, lastMonthEnd);
        stats.ReSpecLastMonth = SumValue(ValueType.ReSpec, lastMonthStart, lastMonthEnd);
        stats.MightLastMonth = SumValue(ValueType.Might, lastMonthStart, lastMonthEnd);
        stats.FavorLastMonth = SumValue(ValueType.Favor, lastMonthStart, lastMonthEnd);
        stats.FactionPointsLastMonth = SumValue(ValueType.FactionPoints, lastMonthStart, lastMonthEnd);
        stats.FameLastMonthPerHour = PerHour(stats.FameLastMonth, lastMonthHours);
        stats.SilverLastMonthPerHour = PerHour(stats.SilverLastMonth, lastMonthHours);
        stats.ReSpecLastMonthPerHour = PerHour(stats.ReSpecLastMonth, lastMonthHours);
        stats.MightLastMonthPerHour = PerHour(stats.MightLastMonth, lastMonthHours);
        stats.FavorLastMonthPerHour = PerHour(stats.FavorLastMonth, lastMonthHours);
        stats.FactionPointsLastMonthPerHour = PerHour(stats.FactionPointsLastMonth, lastMonthHours);
        stats.GatheringValueLastMonth = SumValue(ValueType.GatheringValue, lastMonthStart, lastMonthEnd);
        stats.GatheringValueLastMonthPerHour = PerHour(stats.GatheringValueLastMonth, lastMonthHours);

        // This Year
        var thisYearHours = SumActiveHours(thisYearStart, now);
        stats.FameThisYear = SumValue(ValueType.Fame, thisYearStart, now);
        stats.SilverThisYear = SumValue(ValueType.Silver, thisYearStart, now);
        stats.ReSpecThisYear = SumValue(ValueType.ReSpec, thisYearStart, now);
        stats.MightThisYear = SumValue(ValueType.Might, thisYearStart, now);
        stats.FavorThisYear = SumValue(ValueType.Favor, thisYearStart, now);
        stats.FactionPointsThisYear = SumValue(ValueType.FactionPoints, thisYearStart, now);
        stats.FameThisYearPerHour = PerHour(stats.FameThisYear, thisYearHours);
        stats.SilverThisYearPerHour = PerHour(stats.SilverThisYear, thisYearHours);
        stats.ReSpecThisYearPerHour = PerHour(stats.ReSpecThisYear, thisYearHours);
        stats.MightThisYearPerHour = PerHour(stats.MightThisYear, thisYearHours);
        stats.FavorThisYearPerHour = PerHour(stats.FavorThisYear, thisYearHours);
        stats.FactionPointsThisYearPerHour = PerHour(stats.FactionPointsThisYear, thisYearHours);
        stats.GatheringValueThisYear = SumValue(ValueType.GatheringValue, thisYearStart, now);
        stats.GatheringValueThisYearPerHour = PerHour(stats.GatheringValueThisYear, thisYearHours);

        // Total
        var totalHours = SumActiveHours(DateTime.MinValue, now);
        stats.FameTotal = SumValue(ValueType.Fame, DateTime.MinValue, now);
        stats.SilverTotal = SumValue(ValueType.Silver, DateTime.MinValue, now);
        stats.ReSpecTotal = SumValue(ValueType.ReSpec, DateTime.MinValue, now);
        stats.MightTotal = SumValue(ValueType.Might, DateTime.MinValue, now);
        stats.FavorTotal = SumValue(ValueType.Favor, DateTime.MinValue, now);
        stats.FactionPointsTotal = SumValue(ValueType.FactionPoints, DateTime.MinValue, now);
        stats.FameTotalPerHour = PerHour(stats.FameTotal, totalHours);
        stats.SilverTotalPerHour = PerHour(stats.SilverTotal, totalHours);
        stats.ReSpecTotalPerHour = PerHour(stats.ReSpecTotal, totalHours);
        stats.MightTotalPerHour = PerHour(stats.MightTotal, totalHours);
        stats.FavorTotalPerHour = PerHour(stats.FavorTotal, totalHours);
        stats.FactionPointsTotalPerHour = PerHour(stats.FactionPointsTotal, totalHours);
        stats.GatheringValueTotal = SumValue(ValueType.GatheringValue, DateTime.MinValue, now);
        stats.GatheringValueTotalPerHour = PerHour(stats.GatheringValueTotal, totalHours);

        // Session — mirror from DashboardBindings (maintained by LiveStatsTracker)
        stats.FameSession = bindings.TotalGainedFameInSession;
        stats.SilverSession = bindings.TotalGainedSilverInSession;
        stats.ReSpecSession = bindings.TotalGainedReSpecPointsInSession;
        stats.MightSession = bindings.TotalGainedMightInSession;
        stats.FavorSession = bindings.TotalGainedFavorInSession;
        stats.GatheringValueSession = bindings.TotalGatheredValueInSession;
        stats.GatheringValueSessionPerHour = bindings.GatheringValuePerHour;
        stats.FactionPointsSession = _mainWindowViewModel.FactionPointStats.FirstOrDefault()?.Value ?? 0;
        stats.FameSessionPerHour = bindings.FamePerHour;
        stats.SilverSessionPerHour = bindings.SilverPerHour;
        stats.ReSpecSessionPerHour = bindings.ReSpecPointsPerHour;
        stats.MightSessionPerHour = bindings.MightPerHour;
        stats.FavorSessionPerHour = bindings.FavorPerHour;
        stats.FactionPointsSessionPerHour = _mainWindowViewModel.FactionPointStats.FirstOrDefault()?.ValuePerHour ?? 0;
        stats.CityFaction = _mainWindowViewModel.FactionPointStats.FirstOrDefault()?.CityFaction ?? CityFaction.Unknown;

        // Active time labels per period
        var sessionStart = _trackingController.LiveStatsTracker.SessionStartUtc;
        var sessionTs = sessionStart != default ? DateTime.Now - sessionStart.ToLocalTime() : TimeSpan.Zero;
        stats.ActiveTimeSession = FormatActiveTime(Math.Max(0, sessionTs.TotalHours));
        stats.ActiveTimeToday = FormatActiveTime(todayHours);
        stats.ActiveTimeThisWeek = FormatActiveTime(thisWeekHours);
        stats.ActiveTimeLastWeek = FormatActiveTime(lastWeekHours);
        stats.ActiveTimeThisMonth = FormatActiveTime(thisMonthHours);
        stats.ActiveTimeLastMonth = FormatActiveTime(lastMonthHours);
        stats.ActiveTimeThisYear = FormatActiveTime(thisYearHours);
        stats.ActiveTimeTotal = FormatActiveTime(totalHours);
    }

    private double SumValue(ValueType type, DateTime from, DateTime to)
    {
        return _dashboardStatistics.DailyValues
            .Where(x => x.ValueType == type && x.Date >= from.Date && x.Date <= to.Date)
            .Sum(x => x.Value);
    }

    private double SumActiveHours(DateTime from, DateTime to)
    {
        var seconds = _dashboardStatistics.DailyValues
            .Where(x => x.ValueType == ValueType.ActiveTime && x.Date >= from.Date && x.Date <= to.Date)
            .Sum(x => x.Value);
        return seconds / 3600.0;
    }

    private static double PerHour(double value, double activeHours)
    {
        return activeHours > 0 ? value / activeHours : 0;
    }

    private static string FormatActiveTime(double hours)
    {
        var ts = TimeSpan.FromHours(hours);
        if (ts.TotalMinutes < 1) return string.Empty;
        if (ts.TotalHours < 1) return $"{ts.Minutes}m";
        if (ts.TotalHours < 24) return $"{(int) ts.TotalHours}h {ts.Minutes:D2}m";
        return $"{(int) ts.TotalDays}d {ts.Hours}h";
    }

    #endregion

    #region Load / Save local file data

    public async Task LoadFromFileAsync()
    {
        _dashboardStatistics = await FileController.LoadAsync<DashboardStatistics>(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.StatsFileName));
        UpdateRepairCostsUi();
        UpdateLifetimeStatsUi();
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));
        await FileController.SaveAsync(_dashboardStatistics, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.StatsFileName));
        Log.Information("Statistics saved");
    }

    #endregion
}