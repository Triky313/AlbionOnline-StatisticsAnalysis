using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Manager;

public class StatisticController : IStatisticController
{
    private readonly ITrackingController _trackingController;
    private readonly IEntityController _entityController;
    private readonly MainWindowViewModelOld _mainWindowViewModel;
    private readonly List<ValueType> _valueTypes = new()
    {
        ValueType.Fame, ValueType.Silver, ValueType.ReSpec, ValueType.FactionFame, ValueType.FactionPoints, ValueType.Might, ValueType.Favor, ValueType.RepairCosts
    };

    private DateTime _lastChartUpdate;
    private DashboardStatistics _dashboardStatistics = new();

    public event Action OnAddValue;

    public StatisticController(ITrackingController trackingController,
        IEntityController entityController,
        MainWindowViewModelOld mainWindowViewModel)
    {
        _trackingController = trackingController;
        _entityController = entityController;
        _mainWindowViewModel = mainWindowViewModel;
        InitStartHourValues();

        OnAddValue += UpdateRepairCostsUi;
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
        _mainWindowViewModel.DashboardBindings.KillsToday = _entityController.LocalUserData.KillsToday;
        _mainWindowViewModel.DashboardBindings.SoloKillsToday = _entityController.LocalUserData.SoloKillsToday;
        _mainWindowViewModel.DashboardBindings.DeathsToday = _entityController.LocalUserData.DeathsToday;
        _mainWindowViewModel.DashboardBindings.KillsThisWeek = _entityController.LocalUserData.KillsWeek;
        _mainWindowViewModel.DashboardBindings.SoloKillsThisWeek = _entityController.LocalUserData.SoloKillsWeek;
        _mainWindowViewModel.DashboardBindings.DeathsThisWeek = _entityController.LocalUserData.DeathsWeek;
        _mainWindowViewModel.DashboardBindings.KillsThisMonth = _entityController.LocalUserData.KillsMonth;
        _mainWindowViewModel.DashboardBindings.SoloKillsThisMonth = _entityController.LocalUserData.SoloKillsMonth;
        _mainWindowViewModel.DashboardBindings.DeathsThisMonth = _entityController.LocalUserData.DeathsMonth;

        _mainWindowViewModel.DashboardBindings.AverageItemPowerWhenKilling = _entityController.LocalUserData.AverageItemPowerWhenKilling;
        _mainWindowViewModel.DashboardBindings.AverageItemPowerOfTheKilledEnemies = _entityController.LocalUserData.AverageItemPowerOfTheKilledEnemies;
        _mainWindowViewModel.DashboardBindings.AverageItemPowerWhenDying = _entityController.LocalUserData.AverageItemPowerWhenDying;

        _mainWindowViewModel.DashboardBindings.LastUpdate = _entityController.LocalUserData.LastUpdate;
    }

    #endregion

    #region Repair costs stats

    public void UpdateRepairCostsUi()
    {
        var currentDate = DateTime.Now;

        _mainWindowViewModel.DashboardBindings.RepairCostsToday = _dashboardStatistics?.DailyValues
            ?.Where(x => x.ValueType == ValueType.RepairCosts
                         && x.Date.Year == currentDate.Year
                         && x.Date.Month == currentDate.Month
                         && x.Date.Day == currentDate.Day)
            .Sum(x => FixPoint.FromFloatingPointValue(x.Value).IntegerValue) ?? 0;

        _mainWindowViewModel.DashboardBindings.RepairCostsLast7Days = _dashboardStatistics?.DailyValues
            ?.Where(x => x.ValueType == ValueType.RepairCosts
                         && x.Date.Ticks > currentDate.AddDays(-7).Ticks)
            .Sum(x => FixPoint.FromFloatingPointValue(x.Value).IntegerValue) ?? 0;

        _mainWindowViewModel.DashboardBindings.RepairCostsLast30Days = _dashboardStatistics?.DailyValues
            ?.Where(x => x.ValueType == ValueType.RepairCosts
                         && x.Date.Ticks > currentDate.AddDays(-30).Ticks)
            .Sum(x => FixPoint.FromFloatingPointValue(x.Value).IntegerValue) ?? 0;
    }

    #endregion

    #region Load / Save local file data

    public async Task LoadFromFileAsync()
    {
        FileController.TransferFileIfExistFromOldPathToUserDataDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.StatsFileName));
        _dashboardStatistics = await FileController.LoadAsync<DashboardStatistics>(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.StatsFileName));
        UpdateRepairCostsUi();
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));
        await FileController.SaveAsync(_dashboardStatistics, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.StatsFileName));
        Debug.Print("Statistics saved");
    }

    #endregion
}