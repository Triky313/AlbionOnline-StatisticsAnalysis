using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class StatisticController
    {
        private readonly TrackingController _trackingController;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly ObservableCollection<DashboardHourObject> _stats = new();
        private readonly List<ValueType> _valueTypes = new() { ValueType.Fame, ValueType.Silver, ValueType.ReSpec, ValueType.FactionFame, ValueType.FactionPoints, ValueType.Might, ValueType.Favor };
        private double? _lastReSpecValue;
        private DateTime _lastChartUpdate;

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

        public void AddValue(ValueType type, double gainedValue)
        {
            if (!_trackingController.IsTrackingAllowedByMainCharacter())
            {
                return;
            }

            gainedValue = GetGainedValue(type, gainedValue);

            var dateTimeNow = DateTime.Now;
            var dbHourObject = _stats?.FirstOrDefault(x => x.Type == type);

            var dbHourValues = dbHourObject?.HourValues?.FirstOrDefault(x => x.Date.Date.Equals(dateTimeNow.Date) && x.Hour.Equals(dateTimeNow.Hour));

            if (dbHourValues == null)
            {
                return;
            }

            dbHourValues.Value += gainedValue;

            UpdateHourChart(_stats);
        }

        private void UpdateHourChart(ObservableCollection<DashboardHourObject> stats)
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
    }
}