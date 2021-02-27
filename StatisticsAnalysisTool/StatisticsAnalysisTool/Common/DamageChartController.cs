using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using StatisticsAnalysisTool.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Common
{
    public class DamageChartController
    {
        private readonly SeriesCollection _damageMeterSeriesCollection;
        private Func<double, string> _damageMeterXFormatter;

        public DamageChartController(SeriesCollection damageMeterSeriesCollection, Func<double, string> damageMeterXFormatter)
        {
            _damageMeterSeriesCollection = damageMeterSeriesCollection;
            _damageMeterXFormatter = damageMeterXFormatter;

            _damageMeterSeriesCollection = new SeriesCollection(GetDamageMeterMapper());
        }

        public void AddToDamageMeter(DamageMeterObject damageMeterObject)
        {
            if (string.IsNullOrEmpty(damageMeterObject.Name) || damageMeterObject.Value < 0)
            {
                return;
            }

            if (_damageMeterSeriesCollection.Any(x => x.Title == damageMeterObject.Name))
            {
                var rowSeries = _damageMeterSeriesCollection.FirstOrDefault(x => x.Title == damageMeterObject.Name);
                rowSeries?.Values?.Clear();
                rowSeries?.Values?.Add(new DamageMeterObject() { Name = damageMeterObject.Name, Value = damageMeterObject.Value });
            }
            else
            {
                var rowSeries = new RowSeries
                {
                    Fill = (Brush)Application.Current.Resources["SolidColorBrush.City.Martlock"], // TODO: Color method adjustment
                    Title = damageMeterObject.Name,
                    Values = new ChartValues<DamageMeterObject>(),
                    DataLabels = true,
                    LabelsPosition = BarLabelPosition.Parallel
                };
                rowSeries.Values?.Add(new DamageMeterObject() { Name = damageMeterObject.Name, Value = damageMeterObject.Value });

                _damageMeterSeriesCollection.Add(rowSeries);
            }
        }
        
        private CartesianMapper<DamageMeterObject> GetDamageMeterMapper()
        {
            return Mappers.Xy<DamageMeterObject>()
                .X(value => value.Value);
                //.Y(value => value.Value);
        }
    }
}