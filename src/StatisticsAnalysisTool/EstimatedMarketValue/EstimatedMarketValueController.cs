using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.EstimatedMarketValue;

public static class EstimatedMarketValueController
{
    private static ObservableCollection<EstimatedMarketValueObject> _estimatedMarketValueObjects = new();

    public static void Add(int itemId, long estimatedMarketValueInternal, ItemQuality quality = ItemQuality.Unknown)
    {
        if (itemId <= 0 || estimatedMarketValueInternal <= 0)
        {
            return;
        }

        var item = ItemController.GetItemByIndex(itemId);

        if (item == null)
        {
            return;
        }

        var timestamp = DateTime.UtcNow;
        var estMarketValueObject = _estimatedMarketValueObjects?.FirstOrDefault(x => x.UniqueItemName == item.UniqueName);

        if (estMarketValueObject == null)
        {
            var newEstimatedMarketValueObject = new EstimatedMarketValueObject()
            {
                UniqueItemName = item.UniqueName,
                EstimatedMarketValues = new List<EstQualityValue>()
                {
                    new()
                    {
                        Timestamp = timestamp,
                        MarketValue = FixPoint.FromInternalValue(estimatedMarketValueInternal),
                        Quality = quality
                    }
                }
            };

            _estimatedMarketValueObjects?.Add(newEstimatedMarketValueObject);
            estMarketValueObject = newEstimatedMarketValueObject;
        }
        else
        {
            var estMarketValue = estMarketValueObject.EstimatedMarketValues.FirstOrDefault(x => x.Quality == quality);
            if (estMarketValue == null)
            {
                estMarketValueObject.EstimatedMarketValues.Add(new EstQualityValue
                {
                    Timestamp = timestamp,
                    MarketValue = FixPoint.FromInternalValue(estimatedMarketValueInternal),
                    Quality = quality
                });
            }
            else
            {
                estMarketValue.Timestamp = timestamp;
                estMarketValue.MarketValue = FixPoint.FromInternalValue(estimatedMarketValueInternal);
                estMarketValue.Quality = quality;
            }
        }

        ItemController.SetEstimatedMarketValue(item.UniqueName, estMarketValueObject.EstimatedMarketValues);
    }

    public static async Task SetAllEstimatedMarketValuesToItemsAsync()
    {
        await foreach (var estMarketValue in _estimatedMarketValueObjects.ToAsyncEnumerable())
        {
            ItemController.SetEstimatedMarketValue(estMarketValue.UniqueItemName, estMarketValue.EstimatedMarketValues);
        }
    }

    public static EstQualityValue CalculateNearestToAverage(List<EstQualityValue> estQualityValue)
    {
        if (estQualityValue == null)
        {
            return new EstQualityValue();
        }

        switch (estQualityValue.Count)
        {
            case < 1:
                return new EstQualityValue();
            case < 3:
                return estQualityValue.OrderBy(x => x.MarketValue.IntegerValue).First();
        }

        var sortedList = estQualityValue.OrderBy(x => x.MarketValue.IntegerValue).ToList();
        sortedList.RemoveAt(0);
        sortedList.RemoveAt(sortedList.Count - 1);

        double average = sortedList.Average(x => x.MarketValue.IntegerValue);

        EstQualityValue nearestValue = null;
        double smallestDifference = double.MaxValue;

        foreach (EstQualityValue value in sortedList)
        {
            double difference = Math.Abs(value.MarketValue.IntegerValue - average);

            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                nearestValue = value;
            }
        }

        return nearestValue;
    }

    #region Load / Save local file data

    public static async Task LoadFromFileAsync()
    {
        var estimatedMarketValueDtos = await FileController.LoadAsync<List<EstimatedMarketValueDto>>(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.EstimatedMarketValueFileName));

        var estimatedMarketValueObjects = estimatedMarketValueDtos.Where(x => x.EstimatedMarketValueDtos != null).Select(EstimatesMarketValueMapping.Mapping).ToList();
        _estimatedMarketValueObjects = new ObservableCollection<EstimatedMarketValueObject>(estimatedMarketValueObjects);
    }

    public static async Task SaveInFileAsync()
    {
        await FileController.SaveAsync(_estimatedMarketValueObjects.ToList().Select(EstimatesMarketValueMapping.Mapping),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.EstimatedMarketValueFileName));
    }

    #endregion
}