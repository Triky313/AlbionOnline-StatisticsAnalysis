using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using log4net;
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
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class StatisticController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly TrackingController _trackingController;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly ObservableCollection<DashboardHourObject> _stats = new();
        private readonly List<ValueType> _valueTypes = new() { ValueType.Fame, ValueType.Silver, ValueType.ReSpec, ValueType.FactionFame, ValueType.FactionPoints, ValueType.Might, ValueType.Favor };
        private double? _lastReSpecValue;
        private DateTime _lastChartUpdate;
        private DashboardStatistics _dashboardStatistics = new();

        public StatisticController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;
            _mainWindowViewModel = mainWindowViewModel;
            InitStartHourValues();
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

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _stats.Add(dashboardHourObject);
                });
            }
        }

        public void AddValue(ValueType valueType, double gainedValue)
        {
            if (!_trackingController.IsTrackingAllowedByMainCharacter())
            {
                return;
            }

            gainedValue = GetGainedValue(valueType, gainedValue);

            var dateTimeNow = DateTime.Now;
            var dbHourObject = _stats?.FirstOrDefault(x => x.Type == valueType);

            var dbHourValues = dbHourObject?.HourValues?.FirstOrDefault(x => x.Date.Date.Equals(dateTimeNow.Date) && x.Hour.Equals(dateTimeNow.Hour));

            if (dbHourValues == null)
            {
                return;
            }

            dbHourValues.Value += gainedValue;

            _dashboardStatistics.Add(new DailyValues(valueType, gainedValue, dateTimeNow));

            UpdateDailyChart(_stats);
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
                Labeler = value => new DateTime((long)value).ToString(CultureInfo.CurrentCulture),
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
                    var scbt = (SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.Value.{valueType}.Transparent"];
                    return new SolidColorPaint
                    {
                        Color = new SKColor(scbt.Color.R, scbt.Color.G, scbt.Color.B, scbt.Color.A)
                    };
                }

                var scb = (SolidColorBrush)Application.Current.Resources[$"SolidColorBrush.Value.{valueType}"];
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

        private double GetGainedValue(ValueType type, double gainedValue)
        {
            return type switch
            {
                ValueType.ReSpec => Utilities.AddValue(gainedValue, _lastReSpecValue, out _lastReSpecValue),
                _ => gainedValue
            };
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

        #region Kill / Death View

        public void SetKillsDeathsValues()
        {
            _mainWindowViewModel.DashboardObject.KillsToday = _trackingController.EntityController.LocalUserData.KillsToday;
            _mainWindowViewModel.DashboardObject.DeathsToday = _trackingController.EntityController.LocalUserData.DeathsToday;
            _mainWindowViewModel.DashboardObject.KillsThisWeek = _trackingController.EntityController.LocalUserData.KillsWeek;
            _mainWindowViewModel.DashboardObject.DeathsThisWeek = _trackingController.EntityController.LocalUserData.DeathsWeek;

            _mainWindowViewModel.DashboardObject.LastUpdate = _trackingController.EntityController.LocalUserData.LastUpdate;
        }
        

        #endregion

        #region Load / Save local file data

        public void LoadStatisticsFromFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.StatsFileName}";

            if (File.Exists(localFilePath))
            {
                try
                {
                    var localFileString = File.ReadAllText(localFilePath, Encoding.UTF8);
                    var stats = JsonSerializer.Deserialize<DashboardStatistics>(localFileString) ?? new DashboardStatistics();
                    _dashboardStatistics = stats;
                    return;
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    _dashboardStatistics = new DashboardStatistics();
                    return;
                }
            }

            _dashboardStatistics = new DashboardStatistics();
        }

        public void SaveStatisticsInFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.StatsFileName}";

            try
            {
                var fileString = JsonSerializer.Serialize(_dashboardStatistics);
                File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        #endregion
    }
}