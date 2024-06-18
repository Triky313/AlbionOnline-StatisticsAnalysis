using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common.Converters;

public static class Converter
{
    private const int DefaultGoldPrice = 3000;

    private const double MinReceivedGold = 800;
    private const double MaxReceivedGold = 19000;

    private const double MinGoldPriceInCent = 4.95;
    private const double MaxGoldPriceInCent = 99.95;

    private const double MinOneGoldInCent = MinGoldPriceInCent / MinReceivedGold;
    private const double MaxOneGoldInCent = MaxGoldPriceInCent / MaxReceivedGold;

    private static DateTime _lastGoldUpdate = DateTime.MinValue;
    private static int _currentGoldPrice = DefaultGoldPrice;

    private static bool _getGoldPriceisRunning;

    public static string GoldToDollar(ulong itemSilverPrice)
    {
        if (itemSilverPrice == 0 || _currentGoldPrice == 0)
        {
            return 0.ToString();
        }

        _ = GetCurrentGoldPriceAsync();

        var itemPriceInGold = itemSilverPrice / (ulong) _currentGoldPrice;

        var maxPrice = MinOneGoldInCent * itemPriceInGold;
        var minPrice = MaxOneGoldInCent * itemPriceInGold;

        return $"{minPrice:0.00} - {maxPrice:0.00} $";
    }

    private static async Task GetCurrentGoldPriceAsync()
    {
        if (_lastGoldUpdate.Ticks > DateTime.UtcNow.AddHours(-1).Ticks || _getGoldPriceisRunning)
        {
            return;
        }

        _getGoldPriceisRunning = true;
        var response = await ApiController.GetGoldPricesFromJsonAsync(1, 30);
        if (response == null)
        {
            return;
        }

        var price = response.FirstOrDefault()?.Price ?? DefaultGoldPrice;
        _currentGoldPrice = price;
        _lastGoldUpdate = DateTime.UtcNow;
        _getGoldPriceisRunning = false;
    }

    public static Dictionary<int, TOut> GetValue<TOut>(object parameter) where TOut : struct
    {
        var dictionary = new Dictionary<int, TOut>();

        if (parameter == null)
        {
            return dictionary;
        }

        var valueType = parameter.GetType();
        if (!valueType.IsArray)
        {
            return dictionary;
        }

        var array = (Array) parameter;

        for (int i = 0; i < array.Length; i++)
        {
            var value = array.GetValue(i);
            var convertedValue = ConvertTo<TOut>(value);
            if (convertedValue.HasValue)
            {
                dictionary.Add(i, convertedValue.Value);
            }
        }

        return dictionary;
    }

    private static TOut? ConvertTo<TOut>(object input) where TOut : struct
    {
        if (input == null)
            return null;

        return typeof(TOut).Name switch
        {
            nameof(Byte) => (TOut) (object) input.ObjectToByte(),
            nameof(Int16) => (TOut) (object) input.ObjectToShort(),
            nameof(Int32) => (TOut) (object) input.ObjectToInt(),
            nameof(Int64) => (TOut) (object) input.ObjectToLong(),
            nameof(UInt64) => (TOut) (object) input.ObjectToUlong(),
            nameof(Double) => (TOut) (object) input.ObjectToDouble(),
            _ => null
        };
    }
}