using System;
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

    private static bool _getGoldPriceisRunning = false;
    
    public static string GoldToDollar(ulong itemSilverPrice)
    {
        if (itemSilverPrice == 0 || _currentGoldPrice == 0)
        {
            return 0.ToString();
        }

        GetCurrentGoldPriceAsync();

        var itemPriceInGold = itemSilverPrice / (ulong)_currentGoldPrice;

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
}